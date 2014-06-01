using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinFormsShared
{
    public class ViewControl : UserControl
    {
        private bool _isSetup;

        protected ViewControl()
        {
            HandleCreated += OnHandleCreated;
        }

        private static readonly bool DesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        public bool IsSetup
        {
            get
            {
                return DesignTime || _isSetup;
            }
            protected set
            {
                _isSetup = value;
            }
        }

        protected void Setup()
        {
            IsSetup = true;
        }

        private void OnHandleCreated(object sender, EventArgs eventArgs)
        {
            Debug.Assert(IsSetup);
        }
    }
}