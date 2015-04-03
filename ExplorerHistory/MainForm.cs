using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExplorerHistory
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // This works on my monitor :)
            listView1.Columns[0].Width = listView1.Width - 35;
            Task<Dictionary<string, string>> loadPaths = Task<Dictionary<string, string>>.Run(() => loadTypedPaths());
            loadPaths.ContinueWith((res) =>
            {
                if (res.IsFaulted)
                {
                    MessageBox.Show("Cannot load the items list", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }
                foreach(KeyValuePair<String, String> entry in res.Result)
                {
                    addToListView(listView1, entry);

                }
            });
        }

        private void addToListView(ListView theview, KeyValuePair<string, string> entry)
        {
            if (theview.InvokeRequired)
            {
                IAsyncResult invoke1 = theview.BeginInvoke(new Action(() =>
                {
                    addToListView(theview, entry);
                }));
                theview.EndInvoke(invoke1);
            }
            else
            {
                ListViewItem lvi = new ListViewItem(entry.Value);
                lvi.Tag = entry.Key;
                theview.Items.Add(lvi);
            }
        }

        private Dictionary<string, string> loadTypedPaths()
        {
            // Load the explorer history
            RegistryKey pathsKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths", true);
            string[] valueNames = pathsKey.GetValueNames();

            // Create a dictionary with all paths
            Dictionary<String, String> typedPaths = new Dictionary<string, string>();

            foreach (String k in valueNames)
            {
                string entryValue = pathsKey.GetValue(k) as string;
                typedPaths.Add(k, entryValue);
            }

            return typedPaths;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem delThis in listView1.SelectedItems)
            {
                Task delTask = Task.Run(() =>
                {
                    RegistryKey pathsKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths", true);
                    pathsKey.DeleteValue(delThis.Tag as string);
                });
                delTask.ContinueWith((res) =>
                {
                    if (res.IsFaulted)
                        return;

                    IAsyncResult invoke2 = listView1.BeginInvoke(new Action(() =>
                        {
                            listView1.Items.Remove(delThis);
                        }));
                    listView1.EndInvoke(invoke2);
                });
            }
        }
    }
}
