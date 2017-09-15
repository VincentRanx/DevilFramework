namespace TableGenerater
{
    partial class TableGenerater
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableGenerater));
            this.selectExcelFile = new System.Windows.Forms.OpenFileDialog();
            this.excelFileList = new System.Windows.Forms.ComboBox();
            this.excelFileGroup = new System.Windows.Forms.GroupBox();
            this.btnEditExcelFile = new System.Windows.Forms.Button();
            this.dataPreviewGroup = new System.Windows.Forms.GroupBox();
            this.dataPreview = new System.Windows.Forms.DataGridView();
            this.fieldDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commentDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableSchema = new System.Data.DataSet();
            this.tableFields = new System.Data.DataTable();
            this.dataColumn1 = new System.Data.DataColumn();
            this.dataColumn2 = new System.Data.DataColumn();
            this.dataColumn3 = new System.Data.DataColumn();
            this.csharpOutputGroup = new System.Windows.Forms.GroupBox();
            this.btnGenerateCsharp = new System.Windows.Forms.Button();
            this.csharpOutputList = new System.Windows.Forms.ComboBox();
            this.nameSpaceGroup = new System.Windows.Forms.GroupBox();
            this.namespaceValue = new System.Windows.Forms.TextBox();
            this.jsonOutputGroup = new System.Windows.Forms.GroupBox();
            this.btnGenerateJson = new System.Windows.Forms.Button();
            this.jsonOutputList = new System.Windows.Forms.ComboBox();
            this.selectOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.excelFileGroup.SuspendLayout();
            this.dataPreviewGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableSchema)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableFields)).BeginInit();
            this.csharpOutputGroup.SuspendLayout();
            this.nameSpaceGroup.SuspendLayout();
            this.jsonOutputGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // selectExcelFile
            // 
            this.selectExcelFile.FileName = "TableBase";
            this.selectExcelFile.Filter = "*.xls|*.xlsx";
            this.selectExcelFile.SupportMultiDottedExtensions = true;
            // 
            // excelFileList
            // 
            this.excelFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.excelFileList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.excelFileList.FormattingEnabled = true;
            this.excelFileList.Location = new System.Drawing.Point(6, 18);
            this.excelFileList.Name = "excelFileList";
            this.excelFileList.Size = new System.Drawing.Size(824, 20);
            this.excelFileList.TabIndex = 1;
            this.excelFileList.SelectedIndexChanged += new System.EventHandler(this.excelFileList_SelectedValueChanged);
            // 
            // excelFileGroup
            // 
            this.excelFileGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.excelFileGroup.Controls.Add(this.btnEditExcelFile);
            this.excelFileGroup.Controls.Add(this.excelFileList);
            this.excelFileGroup.Location = new System.Drawing.Point(8, 71);
            this.excelFileGroup.Name = "excelFileGroup";
            this.excelFileGroup.Size = new System.Drawing.Size(927, 50);
            this.excelFileGroup.TabIndex = 4;
            this.excelFileGroup.TabStop = false;
            this.excelFileGroup.Text = "EXCEL 文件";
            // 
            // btnEditExcelFile
            // 
            this.btnEditExcelFile.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnEditExcelFile.Location = new System.Drawing.Point(836, 16);
            this.btnEditExcelFile.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnEditExcelFile.Name = "btnEditExcelFile";
            this.btnEditExcelFile.Size = new System.Drawing.Size(84, 23);
            this.btnEditExcelFile.TabIndex = 0;
            this.btnEditExcelFile.Text = "编辑";
            this.btnEditExcelFile.UseVisualStyleBackColor = true;
            this.btnEditExcelFile.Click += new System.EventHandler(this.btnEditExcelFile_Click);
            // 
            // dataPreviewGroup
            // 
            this.dataPreviewGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataPreviewGroup.Controls.Add(this.dataPreview);
            this.dataPreviewGroup.Location = new System.Drawing.Point(8, 127);
            this.dataPreviewGroup.Name = "dataPreviewGroup";
            this.dataPreviewGroup.Size = new System.Drawing.Size(927, 347);
            this.dataPreviewGroup.TabIndex = 5;
            this.dataPreviewGroup.TabStop = false;
            this.dataPreviewGroup.Text = "预览";
            // 
            // dataPreview
            // 
            this.dataPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataPreview.AutoGenerateColumns = false;
            this.dataPreview.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataPreview.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataPreview.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fieldDataGridViewTextBoxColumn,
            this.typeDataGridViewTextBoxColumn,
            this.commentDataGridViewTextBoxColumn});
            this.dataPreview.DataMember = "Table1";
            this.dataPreview.DataSource = this.tableSchema;
            this.dataPreview.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataPreview.Location = new System.Drawing.Point(6, 20);
            this.dataPreview.Name = "dataPreview";
            this.dataPreview.RowTemplate.Height = 23;
            this.dataPreview.Size = new System.Drawing.Size(915, 321);
            this.dataPreview.TabIndex = 0;
            // 
            // fieldDataGridViewTextBoxColumn
            // 
            this.fieldDataGridViewTextBoxColumn.DataPropertyName = "field";
            this.fieldDataGridViewTextBoxColumn.HeaderText = "属性";
            this.fieldDataGridViewTextBoxColumn.Name = "fieldDataGridViewTextBoxColumn";
            // 
            // typeDataGridViewTextBoxColumn
            // 
            this.typeDataGridViewTextBoxColumn.DataPropertyName = "type";
            this.typeDataGridViewTextBoxColumn.HeaderText = "数据类型";
            this.typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            // 
            // commentDataGridViewTextBoxColumn
            // 
            this.commentDataGridViewTextBoxColumn.DataPropertyName = "comment";
            this.commentDataGridViewTextBoxColumn.HeaderText = "备注";
            this.commentDataGridViewTextBoxColumn.Name = "commentDataGridViewTextBoxColumn";
            // 
            // tableSchema
            // 
            this.tableSchema.DataSetName = "TableBase";
            this.tableSchema.Tables.AddRange(new System.Data.DataTable[] {
            this.tableFields});
            // 
            // tableFields
            // 
            this.tableFields.CaseSensitive = true;
            this.tableFields.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn2,
            this.dataColumn3});
            this.tableFields.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.UniqueConstraint("Constraint1", new string[] {
                        "field"}, true)});
            this.tableFields.PrimaryKey = new System.Data.DataColumn[] {
        this.dataColumn1};
            this.tableFields.TableName = "Table1";
            // 
            // dataColumn1
            // 
            this.dataColumn1.AllowDBNull = false;
            this.dataColumn1.ColumnName = "field";
            // 
            // dataColumn2
            // 
            this.dataColumn2.ColumnName = "type";
            // 
            // dataColumn3
            // 
            this.dataColumn3.ColumnName = "comment";
            // 
            // csharpOutputGroup
            // 
            this.csharpOutputGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.csharpOutputGroup.Controls.Add(this.btnGenerateCsharp);
            this.csharpOutputGroup.Controls.Add(this.csharpOutputList);
            this.csharpOutputGroup.Location = new System.Drawing.Point(8, 480);
            this.csharpOutputGroup.Name = "csharpOutputGroup";
            this.csharpOutputGroup.Size = new System.Drawing.Size(927, 60);
            this.csharpOutputGroup.TabIndex = 6;
            this.csharpOutputGroup.TabStop = false;
            this.csharpOutputGroup.Text = "C# 输出";
            // 
            // btnGenerateCsharp
            // 
            this.btnGenerateCsharp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnGenerateCsharp.Location = new System.Drawing.Point(836, 23);
            this.btnGenerateCsharp.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnGenerateCsharp.Name = "btnGenerateCsharp";
            this.btnGenerateCsharp.Size = new System.Drawing.Size(84, 23);
            this.btnGenerateCsharp.TabIndex = 2;
            this.btnGenerateCsharp.Text = "生成 C#";
            this.btnGenerateCsharp.UseVisualStyleBackColor = true;
            this.btnGenerateCsharp.Click += new System.EventHandler(this.btnGenerateCsharp_Click);
            // 
            // csharpOutputList
            // 
            this.csharpOutputList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.csharpOutputList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.csharpOutputList.FormattingEnabled = true;
            this.csharpOutputList.Location = new System.Drawing.Point(7, 25);
            this.csharpOutputList.Name = "csharpOutputList";
            this.csharpOutputList.Size = new System.Drawing.Size(823, 20);
            this.csharpOutputList.TabIndex = 0;
            this.csharpOutputList.SelectedIndexChanged += new System.EventHandler(this.csharpOutputList_SelectedIndexChanged);
            // 
            // nameSpaceGroup
            // 
            this.nameSpaceGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameSpaceGroup.Controls.Add(this.namespaceValue);
            this.nameSpaceGroup.Location = new System.Drawing.Point(8, 13);
            this.nameSpaceGroup.Name = "nameSpaceGroup";
            this.nameSpaceGroup.Size = new System.Drawing.Size(926, 52);
            this.nameSpaceGroup.TabIndex = 7;
            this.nameSpaceGroup.TabStop = false;
            this.nameSpaceGroup.Text = "命名空间";
            // 
            // namespaceValue
            // 
            this.namespaceValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.namespaceValue.Enabled = false;
            this.namespaceValue.Location = new System.Drawing.Point(7, 21);
            this.namespaceValue.Name = "namespaceValue";
            this.namespaceValue.Size = new System.Drawing.Size(913, 21);
            this.namespaceValue.TabIndex = 0;
            // 
            // jsonOutputGroup
            // 
            this.jsonOutputGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.jsonOutputGroup.Controls.Add(this.btnGenerateJson);
            this.jsonOutputGroup.Controls.Add(this.jsonOutputList);
            this.jsonOutputGroup.Location = new System.Drawing.Point(8, 546);
            this.jsonOutputGroup.Name = "jsonOutputGroup";
            this.jsonOutputGroup.Size = new System.Drawing.Size(927, 49);
            this.jsonOutputGroup.TabIndex = 8;
            this.jsonOutputGroup.TabStop = false;
            this.jsonOutputGroup.Text = "Json 输出";
            // 
            // btnGenerateJson
            // 
            this.btnGenerateJson.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnGenerateJson.Location = new System.Drawing.Point(837, 18);
            this.btnGenerateJson.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.btnGenerateJson.Name = "btnGenerateJson";
            this.btnGenerateJson.Size = new System.Drawing.Size(84, 23);
            this.btnGenerateJson.TabIndex = 3;
            this.btnGenerateJson.Text = "生成 Json";
            this.btnGenerateJson.UseVisualStyleBackColor = true;
            this.btnGenerateJson.Click += new System.EventHandler(this.btnGenerateJson_Click);
            // 
            // jsonOutputList
            // 
            this.jsonOutputList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.jsonOutputList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jsonOutputList.FormattingEnabled = true;
            this.jsonOutputList.Location = new System.Drawing.Point(6, 20);
            this.jsonOutputList.Name = "jsonOutputList";
            this.jsonOutputList.Size = new System.Drawing.Size(823, 20);
            this.jsonOutputList.TabIndex = 3;
            this.jsonOutputList.SelectedIndexChanged += new System.EventHandler(this.jsonOutputList_SelectedIndexChanged);
            // 
            // TableGenerater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(947, 607);
            this.Controls.Add(this.jsonOutputGroup);
            this.Controls.Add(this.nameSpaceGroup);
            this.Controls.Add(this.csharpOutputGroup);
            this.Controls.Add(this.dataPreviewGroup);
            this.Controls.Add(this.excelFileGroup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(600, 480);
            this.Name = "TableGenerater";
            this.Text = "游戏数据生成器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TableGenerater_FormClosing);
            this.excelFileGroup.ResumeLayout(false);
            this.dataPreviewGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableSchema)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableFields)).EndInit();
            this.csharpOutputGroup.ResumeLayout(false);
            this.nameSpaceGroup.ResumeLayout(false);
            this.nameSpaceGroup.PerformLayout();
            this.jsonOutputGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog selectExcelFile;
        private System.Windows.Forms.ComboBox excelFileList;
        private System.Windows.Forms.GroupBox excelFileGroup;
        private System.Windows.Forms.GroupBox dataPreviewGroup;
        private System.Windows.Forms.GroupBox csharpOutputGroup;
        private System.Windows.Forms.DataGridView dataPreview;
        private System.Data.DataSet tableSchema;
        private System.Data.DataTable tableFields;
        private System.Data.DataColumn dataColumn1;
        private System.Data.DataColumn dataColumn2;
        private System.Data.DataColumn dataColumn3;
        private System.Windows.Forms.GroupBox nameSpaceGroup;
        private System.Windows.Forms.TextBox namespaceValue;
        private System.Windows.Forms.ComboBox csharpOutputList;
        private System.Windows.Forms.Button btnGenerateCsharp;
        private System.Windows.Forms.GroupBox jsonOutputGroup;
        private System.Windows.Forms.Button btnGenerateJson;
        private System.Windows.Forms.ComboBox jsonOutputList;
        private System.Windows.Forms.FolderBrowserDialog selectOutputFolder;
        private System.Windows.Forms.Button btnEditExcelFile;
        private System.Windows.Forms.DataGridViewTextBoxColumn fieldDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn commentDataGridViewTextBoxColumn;
    }
}

