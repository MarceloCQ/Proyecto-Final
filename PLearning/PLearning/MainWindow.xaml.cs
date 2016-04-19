using System;
using System.Collections.Generic;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private bool canDrop = false;
        public MainWindow()
        {
            InitializeComponent();
            
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public void AgregarPagina(object sender, EventArgs e)
        {
            InterfazLinea oldReng = ((InterfazLinea)gEstatutos.Children[gEstatutos.Children.Count - 1]);
            oldReng.Type = LineType.None;
            oldReng.AddClicked -= AgregarPagina;

            InterfazLinea newReng = null;

            for (int i = 0; i  < 10; i++)
            {
                newReng = new InterfazLinea();
                newReng.Height = 29;
                newReng.Drop += onDropIntLinea;
                newReng.DragEnter += onDragEnterIntLinea;
                newReng.DragLeave += onDragLeaveIntLinea;
                newReng.LineNo = InterfazLinea.LastLineNo;

                InterfazLinea.LastLineNo++;

                gEstatutos.Children.Add(newReng);
            }

            newReng.Type = LineType.Add;
            newReng.AddClicked += AgregarPagina;

        }

        private void Estatuto_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void Estatuto_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {

                LineType type = (LineType)((Image)sender).Tag;

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("Estatuto", type);
                DragDrop.DoDragDrop((DependencyObject)sender, dragData, DragDropEffects.Move);
            } 
        }

        private void onDragEnterIntLinea(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Estatuto"))
            {

                InterfazLinea intLinea = sender as InterfazLinea;
                LineType typeA = (LineType)e.Data.GetData("Estatuto");

                if (!e.Data.GetDataPresent("Estatuto"))
                {
                    e.Effects = DragDropEffects.None;
                }
                else
                {
                    if (intLinea.Type != LineType.None || !checkLine(typeA, intLinea.LineNo))
                    {
                        intLinea.Background = Brushes.OrangeRed;
                        canDrop = false;
                        e.Effects = DragDropEffects.None;
                    }
                    else
                    {

                        intLinea.Background = Brushes.LightGreen;
                        canDrop = true;


                    }
                }
            }
        }

        private void onDragLeaveIntLinea(object sender, DragEventArgs e)
        {
            InterfazLinea intLinea = sender as InterfazLinea;

            if (e.Data.GetDataPresent("Estatuto"))
            {
                intLinea.Background = Brushes.Transparent;

            }
        }

        private void onDropIntLinea(object sender, DragEventArgs e)
        {
            InterfazLinea intLinea = sender as InterfazLinea;

            if (e.Data.GetDataPresent("Estatuto"))
            {
                if (canDrop)
                {
                    
                    LineType typeA = (LineType)e.Data.GetData("Estatuto");
                    intLinea.Type = typeA;

                    switch (intLinea.Type)
                    {
                        case LineType.If:
                        case LineType.While:
                        case LineType.For:
                        case LineType.Function:
                            int i;
                            for (i = 1; i <= 6; i++ )
                            {
                                InterfazLinea line = gEstatutos.Children[intLinea.LineNo + i] as InterfazLinea;

                                if (line.Type != LineType.None || i == 6)
                                {
                                    i--;
                                    break;
                                }

                                line.IndentLevel++;

                            }

                            InterfazLinea corchCi = gEstatutos.Children[intLinea.LineNo + i] as InterfazLinea;
                            corchCi.LinkedTo = intLinea.LineNo;
                            corchCi.IndentLevel--;
                            corchCi.Type = LineType.Other;
                            corchCi.IsText = true;
                            corchCi.Text = "}";
                            intLinea.LinkedTo = corchCi.LineNo;

                                break;
                    }


                }

                intLinea.Background = Brushes.Transparent;
            }
        }

        private bool checkLine(LineType lineType, int destinationLine)
        {
            bool check = true;

            switch (lineType)
            {
                case LineType.Assign:
                case LineType.Call:
                case LineType.Read:
                case LineType.Write:
                    for (int i = destinationLine + 1; i < gEstatutos.Children.Count; i++)
                    {
                        InterfazLinea lineToCheck = gEstatutos.Children[i] as InterfazLinea;

                        if (lineToCheck.Type != LineType.None)
                        {
                            if (lineToCheck.Type == LineType.Main || lineToCheck.Type == LineType.Function || lineToCheck.Type == LineType.Add || lineToCheck.Type == LineType.Vars)
                            {
                                check = false; 
                                
                            }
                            else
                            {
                                check = true;
                            }
                            break;
                        }

                    }
                    break;
                case LineType.For:
                case LineType.While:
                case LineType.If:

                    if (((InterfazLinea)gEstatutos.Children[destinationLine + 1]).Type == LineType.None)
                    {

                        for (int i = destinationLine + 2; i < gEstatutos.Children.Count; i++)
                        {
                            InterfazLinea lineToCheck = gEstatutos.Children[i] as InterfazLinea;

                            if (lineToCheck.Type != LineType.None)
                            {
                                if (lineToCheck.Type == LineType.Main || lineToCheck.Type == LineType.Function || lineToCheck.Type == LineType.Add || lineToCheck.Type == LineType.Vars)
                                {
                                    check = false;

                                }
                                else
                                {
                                    check = true;
                                }
                                break;
                            }

                        }
                    }
                    else
                    {
                        check = false;
                    }

                    break;
                    
                case LineType.Function:
                    if (((InterfazLinea)gEstatutos.Children[destinationLine + 1]).Type == LineType.None)
                    {
                        for (int i = destinationLine + 2; i < gEstatutos.Children.Count; i++)
                        {
                            InterfazLinea lineToCheck = gEstatutos.Children[i] as InterfazLinea;

                            if (lineToCheck.Type != LineType.None)
                            {
                                if (lineToCheck.Type == LineType.Main || lineToCheck.Type == LineType.Function || lineToCheck.Type == LineType.Add)
                                {
                                    check = true;

                                }
                                else
                                {
                                    check = false;
                                }
                                break;
                            }

                        }
                    }
                    else
                    {
                        check = false;
                    }


                    break;

                case LineType.Vars:
                    for (int i = destinationLine - 1; i >= 0; i--)
                    {
                        InterfazLinea lineToCheck = gEstatutos.Children[i] as InterfazLinea;

                        if (lineToCheck.Type != LineType.None)
                        {
                            if (lineToCheck.Type == LineType.Main || lineToCheck.Type == LineType.Function || lineToCheck.Type == LineType.Vars || lineToCheck.Type == LineType.Program)
                            {
                                check = true;

                            }
                            else
                            {
                                check = false;
                            }
                            break;
                        }

                    }
                    break;


                
            }

            return check;
        }

        private void IntLinea_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void IntLinea_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("IntLinea", sender);
                DragDrop.DoDragDrop((DependencyObject)sender, dragData, DragDropEffects.Move);
            }
        }



        private void imTrashcan_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("IntLinea"))
            {
                InterfazLinea intLinea = e.Data.GetData("IntLinea") as InterfazLinea;
                if (intLinea.Type != LineType.Main && intLinea.Type != LineType.Program && intLinea.Type != LineType.Other)
                {

                    if (intLinea.Type == LineType.While || intLinea.Type == LineType.For || intLinea.Type == LineType.If)
                    {
                        for (int i = intLinea.LineNo + 1; i <= intLinea.LinkedTo; i++)
                        {
                            InterfazLinea intLineaAux = ((InterfazLinea)gEstatutos.Children[i]);
                            

                            if (i == intLinea.LinkedTo)
                            {
                                intLineaAux.Type = LineType.None;
                                intLineaAux.Text = "";
                                intLineaAux.LinkedTo = -1;
                            }
                            else
                            {
                                intLineaAux.IndentLevel--;
                            }

                        }

                        intLinea.LinkedTo = -1;

                    }
                    else if (intLinea.Type == LineType.Function)
                    {
                        for (int i = intLinea.LineNo + 1; i <= intLinea.LinkedTo; i++)
                        {
                            InterfazLinea intLineaAux = ((InterfazLinea)gEstatutos.Children[i]);
                            
                            intLineaAux.Type = LineType.None;

                            if (i == intLinea.LinkedTo)
                            {
                                intLineaAux.Text = "";
                                intLineaAux.LinkedTo = -1;
                            }
                            else
                            {
                                intLineaAux.IndentLevel = intLinea.IndentLevel;
                            }

                        }
                    }
                        

                    intLinea.Type = LineType.None;
                    intLinea.IsText = false;
                    intLinea.Text = "";


                }



                imTrashcan.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/trashcan.png"));

            }
        }

        private void imTrashcan_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("IntLinea"))
            {
                InterfazLinea intLinea = e.Data.GetData("IntLinea") as InterfazLinea;
                if (intLinea.Type == LineType.Main || intLinea.Type == LineType.Program || intLinea.Type == LineType.Other)
                {
                    imTrashcan.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/trashcan-closed.png"));
                }
                else
                {
                    imTrashcan.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/trashcan-open.png"));
                }
                


            }
        }

        private void imTrashcan_DragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("IntLinea"))
            {
                imTrashcan.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/trashcan.png"));
            }
        }

       

        
    }
}
