using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PLearning
{
    /// <summary>
    /// Interaction logic for InterfazLinea.xaml
    /// </summary>
    public partial class InterfazLinea : UserControl
    {

        

        private LineType type;
        private bool isText;
        private string text;
        private int indentLevel;
        public static int LastLineNo = 19;
        public FrameworkElement Actual { get; set; }
        public int LinkedTo { get; set; }

        public event EventHandler AddClicked;
        
        [Description("Type of line"), Category("Data")]
        public LineType Type
        {
            get { return type; }
            set
            {
                if (Actual != null)
                {
                    Actual.Visibility = System.Windows.Visibility.Collapsed;
                }

                type = value;
                if (type == LineType.None)
                {
                    if (Actual != null)
                    {
                        Actual.Visibility = System.Windows.Visibility.Collapsed;
                        Actual = null;
                        isText = false;
                    }
                }
                else
                {
                    if (!isText)
                    {
                        changeActual(type);
                    }
                    else
                    {
                        Actual = tbText;
                    }

                    if (Actual != null)
                    {
                        Actual.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }

        [Description("Text of element"), Category("Data")]
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                tbText.Text = text;

                
            }
        }

        [Description("Check is is Text"), Category("Data")]
        public bool IsText
        {
            get { return isText; }
            set
            {
                isText = value;

                if (Actual != null)
                {
                    Actual.Visibility = System.Windows.Visibility.Collapsed;
                }
               

                if (isText)
                {
                    
                    Actual = tbText;
                    
                }
                else
                {
                    changeActual(type);
                }

                if (Actual != null)
                {
                    Actual.Visibility = System.Windows.Visibility.Visible;
                }
                
               
            }
        }

        [Description("Indenteation level of line"), Category("Data")]
        public int IndentLevel
        {
            get { return indentLevel; }
            set
            {
                indentLevel = value;
                spRenglon.Margin = new Thickness(indentLevel * 20, spRenglon.Margin.Top, spRenglon.Margin.Right, spRenglon.Margin.Bottom);
            }
        }

        [Description("Line number"), Category("Data")]
        public int LineNo { get; set; }
        

        private void changeActual (LineType type)
        {
            switch (type)
            {
                case LineType.Add:
                    Actual = imAdd;
                    break;
                case LineType.Assign:
                    Actual = spEstAsign;
                    break;
                case LineType.Call:
                    Actual = spEstLlamada;
                    break;
                case LineType.For:
                    Actual = spEstFor;
                    break;
                case LineType.Function:
                    Actual = spEstFuncion;
                    break;
                case LineType.If:
                    Actual = spEstIf;
                    break;
                case LineType.Read:
                    Actual = spEstRead;
                    break;
                case LineType.While:
                    Actual = spEstWhile;
                    break;
                case LineType.Write:
                    Actual = spEstWrite;
                    break;
                case LineType.Vars:
                    Actual = spEstVars;
                    break;
                case LineType.Program:
                    Actual = spEstProgram;
                    break;

            }
        }
    

        public InterfazLinea()
        {
            InitializeComponent();
            Type = LineType.None;
            IndentLevel = 2;
            LinkedTo = -1;

        }

        private void imAdd_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.AddClicked != null)
                this.AddClicked(new object(), new EventArgs());
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StackPanel parent = (StackPanel)((FrameworkElement)sender).Parent;
                string texto = "";
                foreach (UIElement child in parent.Children)
                {
                    if (child is TextBox)
                    {
                        texto += ((TextBox)child).Text;
                    }
                    else if (child is TextBlock)
                    {
                        texto += ((TextBlock)child).Text;
                    }
                    else if (child is ComboBox)
                    {
                        texto += ((ComboBox)child).SelectedValue;
                    }

                }

                IsText = true;
                Text = texto;


            }
        }

        private void tbText_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void tbText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (type != LineType.Other || type != LineType.None)
                {
                    IsText = false;
                }
            }
        }

        private void imSupCuant_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StackPanel parent = ((FrameworkElement)sender).Parent as StackPanel;
            parent.Children.Remove(tbCorch1);
            parent.Children.Remove(tbCorch2);
            parent.Children.Remove(tblCuant);
            parent.Children.Remove(imSupCuant);
        }

        private void imSupIgual_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StackPanel parent = ((FrameworkElement)sender).Parent as StackPanel;
            parent.Children.Remove(tblIgual);
            parent.Children.Remove(tbAsign);
            parent.Children.Remove(tbEspacio);
            parent.Children.Remove(imSupIgual);
        }



    }
}
