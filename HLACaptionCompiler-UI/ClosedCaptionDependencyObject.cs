using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HLACaptionCompiler
{
    public class ClosedCaptionDependencyObject : DependencyObject
    {
        public ClosedCaptionDependencyObject()
        {
            Caption = new ClosedCaption();
        }
        public ClosedCaptionDependencyObject(ClosedCaption caption)
        {
            Caption = caption;
            CaptionText = caption.Definition;
            
        }
        public ClosedCaption Caption { get; private set; }


        public static readonly DependencyProperty NameProperty =
           DependencyProperty.Register(nameof(Name), typeof(string),
           typeof(ClosedCaptionDependencyObject), new PropertyMetadata(OnNameChanged));

        private static void OnNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ClosedCaptionDependencyObject me = d as ClosedCaptionDependencyObject;
            if (me != null)
            {
                if (!string.IsNullOrEmpty(me.Name))
                {
                    me.Caption.SoundEvent = me.Name;
                }
                else
                {

                }
            }
        }

        public string Name
        {
            get
            {
                return (string)GetValue(NameProperty);
            }
            set
            {
                this.SetValue(NameProperty, value);
            }
        }




        public static readonly DependencyProperty CaptionTextProperty =
           DependencyProperty.Register(nameof(CaptionText), typeof(string),
           typeof(ClosedCaptionDependencyObject), new PropertyMetadata(OnCaptionTextChanged));

        private static void OnCaptionTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ClosedCaptionDependencyObject me = d as ClosedCaptionDependencyObject;
            if (me != null)
            {
                me.Caption.Definition = me.CaptionText;
            }
        }

        public string CaptionText
        {
            get
            {
                return (string)GetValue(CaptionTextProperty);
            }
            set
            {
                this.SetValue(CaptionTextProperty, value);
            }
        }


    }
}
