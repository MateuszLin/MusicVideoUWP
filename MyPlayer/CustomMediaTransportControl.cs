using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MyPlayer
{
    class CustomMediaTransportControl : MediaTransportControls
    {
        public CustomMediaTransportControl()
        {
            this.DefaultStyleKey = typeof(CustomMediaTransportControl);
        }

        protected override void OnApplyTemplate()
        {
            // This is where you would get your custom button and create an event handler for its click method.
        
            base.OnApplyTemplate();
        }

      
    }


}
