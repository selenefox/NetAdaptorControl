using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Management.PropertyDataCollection;

namespace NetAdaptorControl
{
    public partial class Form1 : Form
    {
        private NetAdapteorInfo currSelectedNetAdapteorInfo = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RefreshAllDevices();
        }

        private void enableButton_Click(object sender, EventArgs e)
        {
            if(currSelectedNetAdapteorInfo != null)
            {
                EnableAdapterByDeviceId(currSelectedNetAdapteorInfo.DeviceId);
                Trace.TraceInformation("EnableAdapterByDeviceId - DeviceId:", currSelectedNetAdapteorInfo.DeviceId);
            }
        }

        private void disableButton_Click(object sender, EventArgs e)
        {
            if (currSelectedNetAdapteorInfo != null)
            {
                DisableAdapterByDeviceId(currSelectedNetAdapteorInfo.DeviceId);
                Trace.TraceInformation("DisableAdapterByDeviceId - DeviceId:", currSelectedNetAdapteorInfo.DeviceId);
            }
        }

        private void NetAdaptor()
        {
            //NetAdaptorControl.Ne
            //获取说有网卡信息
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                Console.WriteLine("All ::" + adapter.Name);
                //判断是否为以太网卡
                //Wireless80211         无线网卡    Ppp     宽带连接
                //Ethernet              以太网卡   
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    Trace.WriteLine(adapter.ToString() + "::" + adapter.Name + "-" + adapter.OperationalStatus);
                }
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    Trace.WriteLine(adapter.ToString() + "::" + adapter.Name + "-" + adapter.OperationalStatus);
                }
            }
        }

        public void RefreshAllDevices()
        {
            
            listView1.BeginUpdate();
            listView1.Items.Clear();
            System.Management.ManagementObjectSearcher moc = new System.Management.ManagementObjectSearcher("Select * from Win32_NetworkAdapter where NetEnabled!=null ");
            foreach (System.Management.ManagementObject mo in moc.Get())
            {
                /*
                if (mo.Properties.Count > 0)
                {
                    // debug tools print mo data all Properties 
                    PropertyDataEnumerator enumerator = mo.Properties.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Type type = enumerator.GetType();
                        PropertyData obj = enumerator.Current;

                        Trace.TraceInformation("Name:{0} Object:{1}", obj.Name, obj.Value);
                    }

                }
                */
                string Manufacturer = mo["Manufacturer"].ToString();
                string DeviceId = mo["DeviceID"].ToString();
                string NetConnectionID = mo["NetConnectionID"].ToString();
                string ProductName = mo["ProductName"].ToString();

                Trace.TraceInformation("Manufacturer={0}, ProductName={1}, NetConnectionID={2}, DeviceId={3}", Manufacturer, ProductName, NetConnectionID, DeviceId);

                listView1.Items.Add(new NetAdaptorListViewItem().BindData(new NetAdapteorInfo(DeviceId,NetConnectionID, Manufacturer, ProductName)));
                
            }
            listView1.EndUpdate();

        }

        /// <summary>
        /// 启用所有适配器
        /// </summary>
        /// <returns></returns>
        public void EnableAdapterByDeviceId(string DeviceId)
        {
            System.Management.ManagementObjectSearcher moc = new System.Management.ManagementObjectSearcher("Select * from Win32_NetworkAdapter where NetEnabled!=null ");
            foreach (System.Management.ManagementObject mo in moc.Get())
            {
                string did = mo["DeviceID"].ToString();
                if (did.Equals(DeviceId))
                {
                    mo.InvokeMethod("Enable", null);
                    break;
                }
            }
        }

        /// <summary>
        /// 禁用所有适配器
        /// </summary>
        public void DisableAdapterByDeviceId(string DeviceId)
        {
            System.Management.ManagementObjectSearcher moc = new System.Management.ManagementObjectSearcher("Select * from Win32_NetworkAdapter where NetEnabled!=null ");
            foreach (System.Management.ManagementObject mo in moc.Get())
            {
                string did = mo["DeviceID"].ToString();
                if (did.Equals(DeviceId))
                {
                    mo.InvokeMethod("Disable", null);
                    break;
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int count = listView1.SelectedItems.Count;
            if(count > 0)
            {
                ButtonSetupEnable(true);
                currSelectedNetAdapteorInfo = ((NetAdaptorListViewItem)listView1.SelectedItems[0]).ItemInfo;
                Trace.TraceInformation("SelectCount:{0}, SelectIndex:{1}", listView1.SelectedItems.Count, listView1.SelectedItems[0].Index);
            }
            else
            {
                ButtonSetupEnable(false);
                currSelectedNetAdapteorInfo = null;
                Trace.TraceInformation("SelectCount:{0}", listView1.SelectedItems.Count);
            }
        }

        private void ButtonSetupEnable(bool enable)
        {
            enableButton.Enabled = enable;
            disableButton.Enabled = enable;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult r = MessageBox.Show("你是否要退出程序吗？“是”退出并关闭程序，“否”将缩小到状态栏。", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (r == DialogResult.Yes)
            {
                notifyIcon1.Visible = false;
                e.Cancel = false;
            }
            else if(r == DialogResult.No)
            {
                this.Hide();
                e.Cancel = true;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshAllDevices();
            notifyIcon1.Visible = true;
        }
    }

    public class NetAdapteorInfo
    {
        public string DeviceId;
        public string NetConnectionID;
        public string Manufacturer;
        public string ProductName;

        public NetAdapteorInfo(string deviceId, string netConnectionID, string manufacturer, string productName)
        {
            DeviceId = deviceId;
            NetConnectionID = netConnectionID;
            Manufacturer = manufacturer;
            ProductName = productName;
        }
    }

    public class NetAdaptorListViewItem : ListViewItem
    {
        public NetAdapteorInfo ItemInfo { get; private set; }

        public NetAdaptorListViewItem BindData(NetAdapteorInfo iteminfo)
        {
            ItemInfo = iteminfo;
            this.Text = ItemInfo.NetConnectionID;
            this.SubItems.Add(ItemInfo.ProductName);
            this.SubItems.Add(ItemInfo.Manufacturer);
            return this;
        }

        public void Refresh(NetAdapteorInfo iteminfo = null)
        {
            if (iteminfo != null)
            {
                ItemInfo = iteminfo;
            }
            this.Text = ItemInfo.NetConnectionID;
            this.SubItems[1].Text = ItemInfo.ProductName;
            this.SubItems[2].Text = ItemInfo.Manufacturer;
        }
    }
}
