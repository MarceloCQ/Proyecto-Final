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



        private LineType type = LineType.None;
        private bool isText;
        private string text;
        private string errorText;
        private int indentLevel;
        public static int LastLineNo = 19;
        public FrameworkElement Actual { get; set; }
        public int LinkedTo { get; set; }

        public event EventHandler AddClicked;
        public event EventHandler SelectionChanged;

        [Description("Type of line"), Category("Data")]
        public LineType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {

                    if (type != LineType.None)
                    {
                        if (Actual != null) Actual.Visibility = Visibility.Collapsed;
                    }

                    if (value == LineType.None)
                    {
                        resetFields();
                    }

                    type = value;

                    changeActual(type);

                    if (type != LineType.None)
                    {

                        if (Actual != null) Actual.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        IsText = false;
                        Actual = null;
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

        [Description("Text of element"), Category("Data")]
        public string ErrorText
        {
            get { return errorText; }
            set
            {
                errorText = value;
                errText.Text = errorText;


            }
        }

        [Description("Check is is Text"), Category("Data")]
        public bool IsText
        {
            get { return isText; }
            set
            {
                if (isText != value)
                {
                    isText = value;

                    if (isText)
                    {

                        if (Actual != null) Actual.Visibility = Visibility.Collapsed;

                        tbText.Visibility = Visibility.Visible;
                    }
                    else
                    {

                        if (Actual != null && Type != LineType.None) Actual.Visibility = Visibility.Visible;

                        tbText.Visibility = Visibility.Collapsed;
                        tbText.Text = "";
                    }
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


        private void changeActual(LineType type)
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
                case LineType.Return:
                    Actual = spEstReturn;
                    break;

            }
        }


        public InterfazLinea()
        {
            InitializeComponent();
            IndentLevel = 1;
            LinkedTo = -1;

        }

        public InterfazLinea Copy()
        {
            InterfazLinea newIntLinea = new InterfazLinea();
            newIntLinea.Type = Type;
            newIntLinea.IsText = IsText;
            newIntLinea.Text = Text;
            newIntLinea.IndentLevel = IndentLevel;
            newIntLinea.LinkedTo = LinkedTo;
            newIntLinea.LineNo = LineNo;

            return newIntLinea;
               
        }

        private void resetFields()
        {

            

            if (Actual != null && type != LineType.Add)
            {
                StackPanel sp = Actual as StackPanel;

                foreach (UIElement child in sp.Children)
                {
                    if (child is TextBox)
                    {
                        ((TextBox)child).Text = "";
                    }
                    else if (child is ComboBox)
                    {
                        ((ComboBox)child).SelectedIndex = -1;
                    }
                }

                if (type == LineType.Vars)
                {
                    tbCorch1.Visibility = Visibility.Visible;
                    tbCorch2.Visibility = Visibility.Visible;
                    tblCuant.Visibility = Visibility.Visible;
                    imSupCuant.Visibility = Visibility.Visible;
                    tblIgual.Visibility = Visibility.Visible;
                    tbAsign.Visibility = Visibility.Visible;
                    tbEspacio.Visibility = Visibility.Visible;
                    imSupIgual.Visibility = Visibility.Visible;
                }

            }

            ErrorText = "";
            LinkedTo = -1;
        }

        private void imAdd_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AddClicked?.Invoke(this, new EventArgs());
        }

        public void convertToText()
        {
            if (!isText)
            {
                string texto = getText();

                IsText = true;
                Text = texto;
            }
        }

        private string getText ()
        {
            string texto = "";
            StackPanel parent = Actual as StackPanel;
            foreach (UIElement child in parent.Children)
            {
                if (child.Visibility == Visibility.Visible)
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

            }

            return texto;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                convertToText();
            }
        }

        private void tbText_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void tbText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (type != LineType.Other && type != LineType.None && type != LineType.Main)
                {
                    IsText = false;
                }
            }
        }

        private void imSupCuant_MouseUp(object sender, MouseButtonEventArgs e)
        {        
            tbCorch1.Visibility = Visibility.Collapsed;
            tbCorch2.Visibility = Visibility.Collapsed;
            tblCuant.Visibility = Visibility.Collapsed;
            imSupCuant.Visibility = Visibility.Collapsed;
        }

        private void imSupIgual_MouseUp(object sender, MouseButtonEventArgs e)
        {         
            tblIgual.Visibility = Visibility.Collapsed;
            tbAsign.Visibility = Visibility.Collapsed;
            tbEspacio.Visibility = Visibility.Collapsed;
            imSupIgual.Visibility = Visibility.Collapsed;
        }

        private void funcType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, new EventArgs());
        }

        override public string ToString()
        {
            string s = "";



            s += LineTypeExtensions.ToString(Type) + "^" + IndentLevel + "^" + LinkedTo + "^";

            if (Type != LineType.None && Type != LineType.Add)
            {
                if (!isText)
                {
                    s += getText() + "^";
                }
                else
                {
                    s += Text + "^";
                }

                if (Actual != null)
                {
                    StackPanel parent = Actual as StackPanel;
                    foreach (UIElement child in parent.Children)
                    {
                        if (child.Visibility == Visibility.Visible)
                        {
                            if (child is TextBox)
                            {
                                s += ((TextBox)child).Text + "^";
                            }
                            else if (child is ComboBox)
                            {
                                s += ((ComboBox)child).SelectedIndex + "^";
                            }
                        }
                        else
                        {
                            s += "?^";
                        }

                    }
                }

            }


            return s;
        }

        public void fromString (string s)
        {
            int parte = 0;
            string[] partes = s.Split(new char[] { '^' });

            Type = LineTypeExtensions.ToType(partes[parte]);
            parte++;

            IndentLevel = int.Parse(partes[parte]);
            parte++;

            LinkedTo = int.Parse(partes[parte]);
            parte++;

            if (Type != LineType.None)
            {
                IsText = true;
                Text = partes[parte];
                parte++;

                if (Actual != null)
                {
                    StackPanel parent = Actual as StackPanel;
                    foreach (UIElement child in parent.Children)
                    {
                        if (partes[parte] != "?")
                        {
                            if (child is TextBox)
                            {
                                ((TextBox)child).Text = partes[parte];
                                parte++;
                            }
                            else if (child is ComboBox)
                            {
                                ((ComboBox)child).SelectedIndex = int.Parse(partes[parte]);
                                parte++;
                            }
                        }
                        else
                        {
                            child.Visibility = Visibility.Collapsed;
                            parte++;
                        }

                    }
                }
            }

            
        }

    }
}
