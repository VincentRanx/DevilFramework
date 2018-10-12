namespace TableGenerator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Support");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("属性列表");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("代码导出");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("数据导出");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tablePreview = new System.Windows.Forms.DataGridView();
            this.dataContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openFileContextMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.reuseTable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.setAsStart = new System.Windows.Forms.ToolStripMenuItem();
            this.genDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cfgContextMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.addCfgFileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCfgMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.mainStatus = new System.Windows.Forms.StatusStrip();
            this.cell = new System.Windows.Forms.ToolStripStatusLabel();
            this.cfgStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.tableGroup = new System.Windows.Forms.GroupBox();
            this.classGroup = new System.Windows.Forms.GroupBox();
            this.modifyNameField = new System.Windows.Forms.TextBox();
            this.cfgTree = new System.Windows.Forms.TreeView();
            this.propertyContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openExcelFile = new System.Windows.Forms.ToolStripMenuItem();
            this.cfgFileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.添加ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.重置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ignoreProperty = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.genCsharpContectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.genDataContectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.chooseFolderContect = new System.Windows.Forms.ToolStripMenuItem();
            this.renameContectMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.tableList = new System.Windows.Forms.ComboBox();
            this.folderBroswerdlg = new System.Windows.Forms.FolderBrowserDialog();
            this.openCfgDlg = new System.Windows.Forms.OpenFileDialog();
            this.rootContainer = new System.Windows.Forms.Panel();
            this.checkDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.tablePreview)).BeginInit();
            this.dataContextMenu.SuspendLayout();
            this.mainStatus.SuspendLayout();
            this.tableGroup.SuspendLayout();
            this.classGroup.SuspendLayout();
            this.propertyContextMenu.SuspendLayout();
            this.rootContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // tablePreview
            // 
            this.tablePreview.AllowUserToAddRows = false;
            this.tablePreview.AllowUserToDeleteRows = false;
            this.tablePreview.BackgroundColor = System.Drawing.SystemColors.Control;
            this.tablePreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tablePreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tablePreview.ColumnHeadersVisible = false;
            this.tablePreview.ContextMenuStrip = this.dataContextMenu;
            this.tablePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePreview.Location = new System.Drawing.Point(5, 24);
            this.tablePreview.Name = "tablePreview";
            this.tablePreview.ReadOnly = true;
            this.tablePreview.RowTemplate.Height = 23;
            this.tablePreview.Size = new System.Drawing.Size(825, 624);
            this.tablePreview.TabIndex = 1;
            this.tablePreview.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.tablePreview_CellMouseEnter);
            // 
            // dataContextMenu
            // 
            this.dataContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileContextMenu,
            this.reuseTable,
            this.toolStripSeparator2,
            this.setAsStart,
            this.genDataMenu,
            this.checkDataMenu,
            this.toolStripSeparator3,
            this.cfgContextMenu});
            this.dataContextMenu.Name = "dataContextMenu";
            this.dataContextMenu.Size = new System.Drawing.Size(181, 170);
            this.dataContextMenu.VisibleChanged += new System.EventHandler(this.dataContextMenu_VisibleChanged);
            // 
            // openFileContextMenu
            // 
            this.openFileContextMenu.Name = "openFileContextMenu";
            this.openFileContextMenu.Size = new System.Drawing.Size(180, 22);
            this.openFileContextMenu.Text = "打开表格";
            this.openFileContextMenu.Click += new System.EventHandler(this.openFileContextMenu_Click);
            // 
            // reuseTable
            // 
            this.reuseTable.Enabled = false;
            this.reuseTable.Name = "reuseTable";
            this.reuseTable.Size = new System.Drawing.Size(180, 22);
            this.reuseTable.Text = "刷新";
            this.reuseTable.Click += new System.EventHandler(this.reuseTable_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // setAsStart
            // 
            this.setAsStart.Enabled = false;
            this.setAsStart.Name = "setAsStart";
            this.setAsStart.Size = new System.Drawing.Size(180, 22);
            this.setAsStart.Text = "设置为起始单元格";
            this.setAsStart.Click += new System.EventHandler(this.setAsStart_Click);
            // 
            // genDataMenu
            // 
            this.genDataMenu.Enabled = false;
            this.genDataMenu.Name = "genDataMenu";
            this.genDataMenu.Size = new System.Drawing.Size(180, 22);
            this.genDataMenu.Text = "生成数据";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // cfgContextMenu
            // 
            this.cfgContextMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCfgFileMenu,
            this.resetCfgMenu});
            this.cfgContextMenu.Name = "cfgContextMenu";
            this.cfgContextMenu.Size = new System.Drawing.Size(180, 22);
            this.cfgContextMenu.Text = "配置文件";
            // 
            // addCfgFileMenu
            // 
            this.addCfgFileMenu.Name = "addCfgFileMenu";
            this.addCfgFileMenu.Size = new System.Drawing.Size(100, 22);
            this.addCfgFileMenu.Text = "添加";
            this.addCfgFileMenu.Click += new System.EventHandler(this.addCfgFileMenu_Click);
            // 
            // resetCfgMenu
            // 
            this.resetCfgMenu.Name = "resetCfgMenu";
            this.resetCfgMenu.Size = new System.Drawing.Size(100, 22);
            this.resetCfgMenu.Text = "重置";
            // 
            // mainStatus
            // 
            this.mainStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cell,
            this.cfgStatus});
            this.mainStatus.Location = new System.Drawing.Point(0, 653);
            this.mainStatus.Name = "mainStatus";
            this.mainStatus.Size = new System.Drawing.Size(1127, 22);
            this.mainStatus.TabIndex = 2;
            this.mainStatus.Text = "statusStrip1";
            // 
            // cell
            // 
            this.cell.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.cell.Name = "cell";
            this.cell.Size = new System.Drawing.Size(37, 17);
            this.cell.Text = "CELL";
            // 
            // cfgStatus
            // 
            this.cfgStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Italic);
            this.cfgStatus.Name = "cfgStatus";
            this.cfgStatus.Size = new System.Drawing.Size(59, 17);
            this.cfgStatus.Text = "data cfg.";
            // 
            // openFileDlg
            // 
            this.openFileDlg.DefaultExt = "xlsx";
            this.openFileDlg.FileName = "Table";
            this.openFileDlg.Filter = "Excel Files|*.xlsx|Excel|*.xls";
            // 
            // tableGroup
            // 
            this.tableGroup.Controls.Add(this.tablePreview);
            this.tableGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableGroup.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tableGroup.Location = new System.Drawing.Point(0, 0);
            this.tableGroup.Name = "tableGroup";
            this.tableGroup.Padding = new System.Windows.Forms.Padding(5);
            this.tableGroup.Size = new System.Drawing.Size(835, 653);
            this.tableGroup.TabIndex = 4;
            this.tableGroup.TabStop = false;
            this.tableGroup.Text = "Table";
            // 
            // classGroup
            // 
            this.classGroup.Controls.Add(this.modifyNameField);
            this.classGroup.Controls.Add(this.cfgTree);
            this.classGroup.Controls.Add(this.tableList);
            this.classGroup.Dock = System.Windows.Forms.DockStyle.Right;
            this.classGroup.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.classGroup.Location = new System.Drawing.Point(835, 0);
            this.classGroup.Name = "classGroup";
            this.classGroup.Padding = new System.Windows.Forms.Padding(5);
            this.classGroup.Size = new System.Drawing.Size(292, 653);
            this.classGroup.TabIndex = 5;
            this.classGroup.TabStop = false;
            this.classGroup.Text = "Properties";
            // 
            // modifyNameField
            // 
            this.modifyNameField.Location = new System.Drawing.Point(31, 219);
            this.modifyNameField.Name = "modifyNameField";
            this.modifyNameField.Size = new System.Drawing.Size(199, 26);
            this.modifyNameField.TabIndex = 3;
            this.modifyNameField.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.modifyNameField_KeyPress);
            this.modifyNameField.Leave += new System.EventHandler(this.modifyNameField_Leave);
            // 
            // cfgTree
            // 
            this.cfgTree.BackColor = System.Drawing.SystemColors.Control;
            this.cfgTree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.cfgTree.ContextMenuStrip = this.propertyContextMenu;
            this.cfgTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cfgTree.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cfgTree.ItemHeight = 30;
            this.cfgTree.Location = new System.Drawing.Point(5, 52);
            this.cfgTree.Name = "cfgTree";
            treeNode1.Name = "Support";
            treeNode1.Text = "Support";
            treeNode2.Name = "Property";
            treeNode2.Text = "属性列表";
            treeNode3.Name = "ClassExp";
            treeNode3.Text = "代码导出";
            treeNode4.Name = "DataExp";
            treeNode4.Text = "数据导出";
            this.cfgTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4});
            this.cfgTree.Size = new System.Drawing.Size(282, 596);
            this.cfgTree.TabIndex = 2;
            this.cfgTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.cfgTree_NodeMouseClick);
            // 
            // propertyContextMenu
            // 
            this.propertyContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openExcelFile,
            this.cfgFileMenu,
            this.ignoreProperty,
            this.toolStripSeparator4,
            this.genCsharpContectMenu,
            this.genDataContectMenu,
            this.toolStripSeparator1,
            this.chooseFolderContect,
            this.renameContectMenu});
            this.propertyContextMenu.Name = "propertyContextMenu";
            this.propertyContextMenu.Size = new System.Drawing.Size(137, 170);
            // 
            // openExcelFile
            // 
            this.openExcelFile.Name = "openExcelFile";
            this.openExcelFile.Size = new System.Drawing.Size(136, 22);
            this.openExcelFile.Text = "打开表格";
            this.openExcelFile.Click += new System.EventHandler(this.openFile_Click);
            // 
            // cfgFileMenu
            // 
            this.cfgFileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.添加ToolStripMenuItem,
            this.重置ToolStripMenuItem});
            this.cfgFileMenu.Name = "cfgFileMenu";
            this.cfgFileMenu.Size = new System.Drawing.Size(136, 22);
            this.cfgFileMenu.Text = "配置文件";
            // 
            // 添加ToolStripMenuItem
            // 
            this.添加ToolStripMenuItem.Name = "添加ToolStripMenuItem";
            this.添加ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.添加ToolStripMenuItem.Text = "添加";
            this.添加ToolStripMenuItem.Click += new System.EventHandler(this.addCfgFileMenu_Click);
            // 
            // 重置ToolStripMenuItem
            // 
            this.重置ToolStripMenuItem.Name = "重置ToolStripMenuItem";
            this.重置ToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.重置ToolStripMenuItem.Text = "重置";
            this.重置ToolStripMenuItem.Click += new System.EventHandler(this.resetCfg_Click);
            // 
            // ignoreProperty
            // 
            this.ignoreProperty.Name = "ignoreProperty";
            this.ignoreProperty.Size = new System.Drawing.Size(136, 22);
            this.ignoreProperty.Text = "忽略属性";
            this.ignoreProperty.Click += new System.EventHandler(this.ignoreProperty_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(133, 6);
            // 
            // genCsharpContectMenu
            // 
            this.genCsharpContectMenu.Name = "genCsharpContectMenu";
            this.genCsharpContectMenu.Size = new System.Drawing.Size(136, 22);
            this.genCsharpContectMenu.Text = "导出 C# 类";
            this.genCsharpContectMenu.Click += new System.EventHandler(this.generateCsharpCode_Click);
            // 
            // genDataContectMenu
            // 
            this.genDataContectMenu.Name = "genDataContectMenu";
            this.genDataContectMenu.Size = new System.Drawing.Size(136, 22);
            this.genDataContectMenu.Text = "导出数据";
            this.genDataContectMenu.Click += new System.EventHandler(this.genDataContectMenu_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(133, 6);
            // 
            // chooseFolderContect
            // 
            this.chooseFolderContect.Name = "chooseFolderContect";
            this.chooseFolderContect.Size = new System.Drawing.Size(136, 22);
            this.chooseFolderContect.Text = "选择文件夹";
            this.chooseFolderContect.Click += new System.EventHandler(this.chooseFolderContect_Click);
            // 
            // renameContectMenu
            // 
            this.renameContectMenu.Name = "renameContectMenu";
            this.renameContectMenu.Size = new System.Drawing.Size(136, 22);
            this.renameContectMenu.Text = "重命名";
            this.renameContectMenu.Click += new System.EventHandler(this.renameContectMenu_Click);
            // 
            // tableList
            // 
            this.tableList.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tableList.FormattingEnabled = true;
            this.tableList.Items.AddRange(new object[] {
            "等待打开表格..."});
            this.tableList.Location = new System.Drawing.Point(5, 24);
            this.tableList.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
            this.tableList.Name = "tableList";
            this.tableList.Size = new System.Drawing.Size(282, 28);
            this.tableList.TabIndex = 1;
            this.tableList.SelectedIndexChanged += new System.EventHandler(this.tableList_SelectedIndexChanged);
            // 
            // openCfgDlg
            // 
            this.openCfgDlg.DefaultExt = "xlsx";
            this.openCfgDlg.FileName = "Table";
            this.openCfgDlg.Filter = "Xml Files|*.xml";
            // 
            // rootContainer
            // 
            this.rootContainer.Controls.Add(this.tableGroup);
            this.rootContainer.Controls.Add(this.classGroup);
            this.rootContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootContainer.Location = new System.Drawing.Point(0, 0);
            this.rootContainer.Name = "rootContainer";
            this.rootContainer.Size = new System.Drawing.Size(1127, 653);
            this.rootContainer.TabIndex = 2;
            // 
            // checkDataMenu
            // 
            this.checkDataMenu.Enabled = false;
            this.checkDataMenu.Name = "checkDataMenu";
            this.checkDataMenu.Size = new System.Drawing.Size(180, 22);
            this.checkDataMenu.Text = "校验数据";
            this.checkDataMenu.Click += new System.EventHandler(this.checkDataMenu_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1127, 675);
            this.Controls.Add(this.rootContainer);
            this.Controls.Add(this.mainStatus);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "表格转化工具";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.tablePreview)).EndInit();
            this.dataContextMenu.ResumeLayout(false);
            this.mainStatus.ResumeLayout(false);
            this.mainStatus.PerformLayout();
            this.tableGroup.ResumeLayout(false);
            this.classGroup.ResumeLayout(false);
            this.classGroup.PerformLayout();
            this.propertyContextMenu.ResumeLayout(false);
            this.rootContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView tablePreview;
        private System.Windows.Forms.StatusStrip mainStatus;
        private System.Windows.Forms.ToolStripStatusLabel cell;
        private System.Windows.Forms.OpenFileDialog openFileDlg;
        private System.Windows.Forms.GroupBox tableGroup;
        private System.Windows.Forms.ContextMenuStrip dataContextMenu;
        private System.Windows.Forms.ToolStripMenuItem setAsStart;
        private System.Windows.Forms.GroupBox classGroup;
        private System.Windows.Forms.FolderBrowserDialog folderBroswerdlg;
        private System.Windows.Forms.ToolStripStatusLabel cfgStatus;
        private System.Windows.Forms.OpenFileDialog openCfgDlg;
        private System.Windows.Forms.ToolStripMenuItem reuseTable;
        private System.Windows.Forms.ToolStripMenuItem openFileContextMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem genDataMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem cfgContextMenu;
        private System.Windows.Forms.ToolStripMenuItem addCfgFileMenu;
        private System.Windows.Forms.ToolStripMenuItem resetCfgMenu;
        private System.Windows.Forms.ComboBox tableList;
        private System.Windows.Forms.TreeView cfgTree;
        private System.Windows.Forms.ContextMenuStrip propertyContextMenu;
        private System.Windows.Forms.ToolStripMenuItem ignoreProperty;
        private System.Windows.Forms.ToolStripMenuItem chooseFolderContect;
        private System.Windows.Forms.ToolStripMenuItem renameContectMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem cfgFileMenu;
        private System.Windows.Forms.ToolStripMenuItem 添加ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 重置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem genDataContectMenu;
        private System.Windows.Forms.ToolStripMenuItem genCsharpContectMenu;
        private System.Windows.Forms.ToolStripMenuItem openExcelFile;
        private System.Windows.Forms.TextBox modifyNameField;
        private System.Windows.Forms.Panel rootContainer;
        private System.Windows.Forms.ToolStripMenuItem checkDataMenu;
    }
}

