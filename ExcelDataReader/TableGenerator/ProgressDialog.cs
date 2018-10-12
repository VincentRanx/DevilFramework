using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TableGenerator
{
    public partial class ProgressDialog : Form
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        public int Progress
        {
            get { return progress.Value; }
            set
            {
                Invoke(new Action<int>(ValidateProgress), value);
            }
        }

        public string DisplayText
        {
            get { return content.Text; }
            set
            {
                Invoke(new Action<string>(ValidateText), value);
            }
        }

        public void Stop(DialogResult result)
        {
            Invoke(new Action<DialogResult>(StopProgress), result);
        }

        void ValidateProgress(int value)
        {
            if (progress.Value != value)
            {
                progress.Value = value;
            }
        }

        void ValidateText(string text)
        {
            if (content.Text != text)
            {
                content.Text = text;
            }
        }
        
        void StopProgress(DialogResult result)
        {
            DialogResult = result;
        }

        public DialogResult ShowWithText(string text)
        {
            content.Text = text;
            return ShowDialog();
        }
    }
}
