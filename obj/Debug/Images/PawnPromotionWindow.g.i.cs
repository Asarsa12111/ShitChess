// Updated by XamlIntelliSenseFileGenerator 23.03.2025 18:59:53
#pragma checksum "..\..\..\Images\PawnPromotionWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "36E5901C58160AC2DE3B6296E82260AA769EDE42952075C3C604EAB5D818BE85"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ChessApp1.Images
{


    /// <summary>
    /// PawnPromotionWindow
    /// </summary>
    public partial class PawnPromotionWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector
    {

#line default
#line hidden

        private bool _contentLoaded;

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ChessApp1;component/images/pawnpromotionwindow.xaml", System.UriKind.Relative);

#line 1 "..\..\..\Images\PawnPromotionWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);

#line default
#line hidden
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.QueenButton = ((System.Windows.Controls.Button)(target));

#line 41 "..\..\..\Images\PawnPromotionWindow.xaml"
                    this.QueenButton.Click += new System.Windows.RoutedEventHandler(this.QueenButton_Click);

#line default
#line hidden
                    return;
                case 2:
                    this.RookButton = ((System.Windows.Controls.Button)(target));

#line 47 "..\..\..\Images\PawnPromotionWindow.xaml"
                    this.RookButton.Click += new System.Windows.RoutedEventHandler(this.RookButton_Click);

#line default
#line hidden
                    return;
                case 3:
                    this.BishopButton = ((System.Windows.Controls.Button)(target));

#line 53 "..\..\..\Images\PawnPromotionWindow.xaml"
                    this.BishopButton.Click += new System.Windows.RoutedEventHandler(this.BishopButton_Click);

#line default
#line hidden
                    return;
                case 4:
                    this.KnightButton = ((System.Windows.Controls.Button)(target));

#line 59 "..\..\..\Images\PawnPromotionWindow.xaml"
                    this.KnightButton.Click += new System.Windows.RoutedEventHandler(this.KnightButton_Click);

#line default
#line hidden
                    return;
            }
            this._contentLoaded = true;
        }
    }
}

