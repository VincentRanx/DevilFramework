using Devil.AI;
using Devil.Utility;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DevilEditor
{
    public static class AIModules
    {
        public const int CATE_COMPOSITE = 0;
        public const int CATE_TASK = 1;
        public const int CATE_CONDITION = 2;
        public const int CATE_SERVICE = 3;
        
        public readonly static string[] CATES = { "控制节点", "任务节点", "条件", "服务" };

        public class Module
        {
            BTCompositeAttribute composite;
            public BTCompositeAttribute Attribute { get { return composite; } }
            System.Type type;
            public System.Type ModuleType { get { return type; } }
            public int CategoryId { get; private set; }
            public string Category { get; private set; }
            public GUIContent Path { get; private set; }
            public KeyCode Hotkey { get; private set; }

            public Color color { get; private set; }
            public Texture icon { get; private set; }
            public string Title { get; private set; }
            public string Detail { get; private set; }
            public string CateTitle { get; private set; }

            public bool IsController { get { return CategoryId == CATE_COMPOSITE; } }
            public bool IsTask { get { return CategoryId == CATE_TASK; } }
            public bool IsCondition { get { return CategoryId == CATE_CONDITION; } }
            public bool IsService { get { return CategoryId == CATE_SERVICE; } }

            private Module(int cate, System.Type type, string defaultIcon)
            {
                var category = CATES[cate];
                this.type = type;
                this.CategoryId = cate;
                this.Category = category;
                composite = Ref.GetCustomAttribute<BTCompositeAttribute>(type);
                Hotkey = composite == null ? 0 : composite.HotKey;
                Title = composite == null || string.IsNullOrEmpty(composite.Title) ? type.Name : composite.Title;
                Detail = composite == null ? null : composite.Detail;
                Color c;
                if (composite != null && !string.IsNullOrEmpty(composite.color) && ColorUtility.TryParseHtmlString(composite.color, out c))
                    color = c;
                else
                    color = AIModules.GetColor(category);
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(composite == null || string.IsNullOrEmpty(composite.IconPath) ? defaultIcon : composite.IconPath);
                var buf = StringUtil.GetBuilder();
                buf.Append(category).Append('/');
                if (composite != null && !string.IsNullOrEmpty(composite.Category))
                    CateTitle = StringUtil.Concat(composite.Category, '/', Title);
                else if (!string.IsNullOrEmpty(type.Namespace) && type.Namespace != "Devil.AI")
                    CateTitle = StringUtil.Concat(type.Namespace, '/', Title);
                else
                    CateTitle = Title;
                buf.Append(CateTitle);
                Path = new GUIContent(StringUtil.ReleaseBuilder(buf));
            }

            public static Module GetModule(System.Type type)
            {
                if (type == null || type.IsAbstract)
                    return null;
                if (type.IsSubclassOf(typeof(BTTaskAsset)))
                {
                    return new Module(CATE_TASK, type, "Assets/DevilFramework/Gizmos/AI Icons/Task Icon.png");
                }
                else if (type.IsSubclassOf(typeof(BTControllerAsset)))
                {
                    return new Module(CATE_COMPOSITE, type, "Assets/DevilFramework/Gizmos/AI Icons/Composite Icon.png");
                }
                else if (type.IsSubclassOf(typeof(BTConditionAsset)))
                {
                    return new Module(CATE_CONDITION, type, "Assets/DevilFramework/Gizmos/AI Icons/Condition Icon.png");
                }
                else if (type.IsSubclassOf(typeof(BTServiceAsset)))
                {
                    return new Module(CATE_SERVICE, type, "Assets/DevilFramework/Gizmos/AI Icons/Service Icon.png");
                }
                else
                {
                    return null;
                }
            }
        }

        public static Texture2D icon;
        static List<Module> sAllModules = new List<Module>();
        public static List<Module> Modules { get { return sAllModules; } }
        static List<Module>[] sModules = new List<Module>[4];
        public static List<Module> GetModules(int cate) { return sModules[cate]; }
        public static event System.Action OnReloead = () => { };
        static Dictionary<string, Color> sColors = new Dictionary<string, Color>();

        static List<System.Type> sSharedTypes = new List<System.Type>();
        public static List<System.Type> SharedTypes { get { return sSharedTypes; } }
        static string[] sSharedTypeNames;
        public static string[] SharedTypeNames { get { return sSharedTypeNames; } }

        static Texture2D sRunIcon;
        public static Texture2D RunIcon
        {
            get
            {
                if (sRunIcon == null)
                    sRunIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/DevilFramework/Gizmos/AI Icons/run.png");
                return sRunIcon;
            }
        }
        static Texture2D sGoodIcon;
        public static Texture2D GoodIcon
        {
            get
            {
                if (sGoodIcon == null)
                    sGoodIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/DevilFramework/Gizmos/AI Icons/good.png");
                return sGoodIcon;
            }
        }
        static Texture2D sBadIcon;
        public static Texture2D BadIcon
        {
            get
            {
                if (sBadIcon == null)
                    sBadIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/DevilFramework/Gizmos/AI Icons/bad.png");
                return sBadIcon;
            }
        }

        public static Color GetColor(string category)
        {
            Color c;
            if (!sColors.TryGetValue(category, out c))
                c = new Color(0.7f, 0.7f, 0.7f);
            return c;
        }

        public static Module Get(System.Type type)
        {
            foreach (var t in sAllModules)
            {
                if (t.ModuleType == type)
                    return t;
            }
            return null;
        }

        public static Module Get(BTNode node)
        {
            if (node == null)
                return null;
            if (node.Asset != null)
                return Get(node.Asset.GetType());
            foreach (var t in sAllModules)
            {
                if (t.ModuleType.Name == node.ModName)
                    return t;
            }
            return null;
        }

        public static void Get(ICollection<Module> modules, KeyCode hotkey)
        {
            if (hotkey == 0)
                return;
            int cate = -1;
            if (hotkey == KeyCode.Alpha1)
                cate = CATE_COMPOSITE;
            else if (hotkey == KeyCode.Alpha2)
                cate = CATE_TASK;
            else if (hotkey == KeyCode.Alpha3)
                cate = CATE_CONDITION;
            else if (hotkey == KeyCode.Alpha4)
                cate = CATE_SERVICE;
            foreach (var t in sAllModules)
            {
                if (t.Hotkey == hotkey || t.CategoryId == cate)
                    modules.Add(t);
            }
        }

        public static void LoadModules()
        {
            sColors[CATES[CATE_COMPOSITE]] = new Color(0.3f, 0.3f, 0.3f);
            sColors[CATES[CATE_TASK]] = new Color(0.6f, 0.1f, 1f);
            sColors[CATES[CATE_SERVICE]] = new Color(0.1f, 0.35f, 0.1f);
            sColors[CATES[CATE_CONDITION]] = new Color(0.1f, 0.3f, 0.5f);

            sSharedTypes.Clear();
            sSharedTypes.Add(typeof(GameObject));
            sSharedTypes.Add(typeof(Transform));
            sSharedTypes.Add(typeof(float));
            sSharedTypes.Add(typeof(bool));
            sSharedTypes.Add(typeof(int));
            sSharedTypes.Add(typeof(Vector3));
            sSharedTypes.Add(typeof(string));

            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/DevilFramework/Gizmos/AI Icons/BehaviourTree Icon.png");
            for (int i = 0; i < sModules.Length; i++)
            {
                sModules[i] = new List<Module>();
            }
            sAllModules.Clear();

            List<System.Type> lst = new List<System.Type>();
            Ref.GetAssemblyTypes(lst);
            foreach(var tp in lst)
            {
                var mod = Module.GetModule(tp);
                if (mod != null)
                {
                    sAllModules.Add(mod);
                    sModules[mod.CategoryId].Add(mod);
                }
                var bts = Ref.GetCustomAttribute<BTSharedTypeAttribute>(tp);
                if (bts != null)
                {
                    sSharedTypes.Add(tp);
                }
            }
            
            sSharedTypes.Add(typeof(object));
            sSharedTypeNames = new string[sSharedTypes.Count];
            for (int i = 0; i < sSharedTypeNames.Length; i++)
            {
                sSharedTypeNames[i] = sSharedTypes[i].FullName;
            }
            BehaviourTreeEditor.InitModules();
            OnReloead();
        }
    }
}