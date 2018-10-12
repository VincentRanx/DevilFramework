using Newtonsoft.Json.Linq;
using TableCore.Exceptions;

namespace TableCore
{
    public enum EDataMode
    {
        list,
        dictionary,
    }

    public class GenData
    {
        int _id;
        public int id
        {
            get { return _id; }
            set
            {
                _id = value;
                mData[mMod.IdIndex] = value.ToString();
            }
        }

        ClassModel mMod;
        string[] mData;
        JObject mFormatData;
        bool mDirty;

        public GenData(ClassModel mod)
        {
            mMod = mod;
            mDirty = true;
            mData = new string[mod.PropertyCount];
        }

        public GenData Copy(int newid)
        {
            var data = new GenData(mMod);
            if(mData.Length > 0)
                System.Array.Copy(mData, data.mData, mData.Length);
            data.id = newid;
            return data;
        }

        public void SetProperty(int index, string data, string cell)
        {
            var pro = mMod.Properties[index].GenType;
            var dt = pro.FormatInput(data);
            if(index == mMod.IdIndex)
            {
                int i;
                if (!int.TryParse(dt, out i))
                    throw new ValueNotMatchTypeException(data, mMod.Properties[index].GenType, cell);
                _id = i;
            }
            if(!pro.IsValid(dt))
                throw new ValueNotMatchTypeException(data, pro, cell);
            mData[index] = dt;
            mDirty = true;
        }

        public void SetProperty(string pname, string data, string cell)
        {
            if(Utils.EqualIgnoreCase(pname, "id"))
            {
                SetProperty(mMod.IdIndex, data, cell);
                return;
            }
            int i = mMod.IndexOfProperty(pname);
            if (i == -1)
                return;
            SetProperty(i, data, cell);
        }

        public string GetProperty(int index)
        {
            return mData[index];
        }
        
        public string GetProperty(string pname)
        {
            int i = mMod.IndexOfProperty(pname);
            if (i == -1)
                return null;
            else
                return GetProperty(i);
        }

        public JObject GetFormatData(GTOutputCfg category)
        {
            if (mDirty || mFormatData == null)
            {
                mDirty = false;
                mFormatData = new JObject();
                for (int i = 0; i < mData.Length; i++)
                {
                    var pro = mMod.Properties[i];
                    if (pro.Ignore)
                        continue;
                    mFormatData[pro.Name] = pro.GenType.Format(mData[i], category);
                }
            }
            return mFormatData;
        }
    }
}
