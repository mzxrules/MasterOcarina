using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Item = ActorDatabaseDefinitionItem;
using ItemOptions = ActorDatabaseDefinitionItemOption;

namespace XmlActorBuilder
{
    public partial class OptionsForm : Form
    {
        public Item Item { get; set; }
        public ItemOptions[] Options
        {
            get 
            {
                if (optionsField == null)
                    return null;
                return optionsField.ToArray(); 
            }
            set
            {
                if (value == null)
                    optionsField = null;
                else
                    optionsField = value.ToList();
            }
        }
        private List<ItemOptions> optionsField = new();
        public OptionsForm()
        {
            InitializeComponent();
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            StringBuilder text = new();
            if (Item != null)
            {
                Text = $"Edit Options: {(string.IsNullOrEmpty(Item.Description) ? "" : Item.Description)}";
            }

            if (Options != null)
            {
                foreach (ItemOptions option in optionsField)
                {
                    text.AppendFormat("{0} {1}{2}", option.Value, option.Description, Environment.NewLine);
                }
                outRichTextBox.Text = text.ToString();
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            List<ItemOptions> newOptions = new();
            if (UpdateList(ref newOptions))
            {
                optionsField = newOptions;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool UpdateList(ref List<ItemOptions> items)
        {
            ItemOptions workingOption;
            string[] result;
            result = outRichTextBox.Text.Split
                (new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in result)
            {
                workingOption = new ItemOptions();
                items.Add(workingOption);
                if (item.Contains(' '))
                {
                    string[] valueAndDescription = item.Split(new char[]{' '}, 2);
                    workingOption.Value = valueAndDescription[0].Trim();
                    workingOption.Description = valueAndDescription[1].Trim();
                }
                else
                {
                    workingOption.Value = item.Trim();
                    workingOption.Description = " ";
                }
            }
            if (items.Count == 0)
                items = null;
            return true;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
    public class ItemOptionsEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            Item item;
            ItemOptions[] itemOptions;
            OptionsForm optionsForm = new();
            DialogResult result;

            item = (Item)context.Instance;
            itemOptions = (ItemOptions[])value;

            optionsForm.Item = item;
            optionsForm.Options = itemOptions;

            result = optionsForm.ShowDialog();
            if (result != DialogResult.OK)
                return base.EditValue(context, provider, value);

            if (optionsForm.Options != null)
            {
                item.SetControlType(Item.Control.ComboBox);
            }

            return optionsForm.Options;
        }
    }
}
