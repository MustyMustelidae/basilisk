using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsShared
{
    public class ViewControl : UserControl
    {
        public bool IsSetup { get; private set; }
        protected ViewControl()
        {
            VisibleChanged += OnVisibleChanged;
        }

        protected void Setup()
        {
            IsSetup = true;
        }

        private void OnVisibleChanged(object sender, EventArgs eventArgs)
        {
            Contract.Requires(IsSetup);
        }
    }
}
