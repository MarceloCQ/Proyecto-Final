using Microsoft.Win32;
using PLearning_Backend;
using PLearning_Backend.Model;
using PLearning_Backend.Structures;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private int lineWithError = -1;
        
        public MainWindow()
        {
            InitializeComponent();
            
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public void AgregarPagina(object sender, EventArgs e)
        {
            addLines(10);
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
            e.Handled = true;
            if (e.Data.GetDataPresent("Estatuto"))
            {

                InterfazLinea intLinea = sender as InterfazLinea;
                LineType typeA = (LineType)e.Data.GetData("Estatuto");

                if (intLinea.Type != LineType.None || !checkLine(typeA, intLinea.LineNo, -1))
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
            else if (e.Data.GetDataPresent("IntLinea"))
            {
                InterfazLinea source = (InterfazLinea)e.Data.GetData("IntLinea");
                InterfazLinea destination = sender as InterfazLinea;

                if (source.Type == LineType.None)
                {
                    e.Effects = DragDropEffects.None;
                }
                else if (source.Type == LineType.Program || destination.Type != LineType.None || !checkLine(source.Type, destination.LineNo, source.LineNo))
                {
                    destination.Background = Brushes.OrangeRed;
                    canDrop = false;
                    e.Effects = DragDropEffects.None;
                }
                else
                {
                    destination.Background = Brushes.LightGreen;
                    canDrop = true;
                }



            }
        }

        private void onDragLeaveIntLinea(object sender, DragEventArgs e)
        {
            InterfazLinea intLinea = sender as InterfazLinea;

            if (e.Data.GetDataPresent("Estatuto") || e.Data.GetDataPresent("IntLinea"))
            {
                intLinea.Background = Brushes.Transparent;

            }
        }

        private void onDropIntLinea(object sender, DragEventArgs e)
        {
            InterfazLinea intLineaDest = sender as InterfazLinea;

            if (e.Data.GetDataPresent("Estatuto"))
            {
                if (canDrop)
                {
                    
                    LineType typeA = (LineType)e.Data.GetData("Estatuto");
                    intLineaDest.Type = typeA;
                    processEst(intLineaDest, false);
                }

                intLineaDest.Background = Brushes.Transparent;
            }
            else if (e.Data.GetDataPresent("IntLinea"))
            {
                if (canDrop)
                {
                    InterfazLinea intLineaSrc = (InterfazLinea)e.Data.GetData("IntLinea");

                    InterfazLinea aux = intLineaSrc.Copy();
                    removeLine(intLineaSrc.LineNo);

                    copyLine(aux, intLineaDest.LineNo, false);

                    if (intLineaDest.Type == LineType.Other && intLineaDest.Text == "}")
                    {
                        changeIndentation(aux, intLineaDest);
                    }

                    processEst(intLineaDest, true);
                    

                }

                intLineaDest.Background = Brushes.Transparent;
            }

        }

        private bool checkLine(LineType lineType, int destinationLine, int sourceLine)
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
                        InterfazLinea intLinea = gEstatutos.Children[i] as InterfazLinea;
                        LineType type = intLinea.Type;

                        if (sourceLine == intLinea.LineNo) type = LineType.None;

                        if (type != LineType.None)
                        {
                            if (type == LineType.Main || type == LineType.Function || type == LineType.Add || type == LineType.Vars)
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

                   

                    for (int i = destinationLine + 1; i < gEstatutos.Children.Count; i++)
                    {
                        InterfazLinea lineToCheck = gEstatutos.Children[i] as InterfazLinea;
                        LineType type = lineToCheck.Type;

                        if (lineToCheck.LineNo == sourceLine) type = LineType.None;

                        if (type != LineType.None)
                        {
                            if (type == LineType.Main || type == LineType.Function || type == LineType.Add || type == LineType.Vars)
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
                    
                case LineType.Function:

                    for (int i = destinationLine + 1; i < gEstatutos.Children.Count; i++)
                    {
                        InterfazLinea lineToCheck = gEstatutos.Children[i] as InterfazLinea;
                        LineType type = lineToCheck.Type;

                        if (lineToCheck.LineNo == sourceLine) type = LineType.None;

                        if (lineToCheck.Type != LineType.None)
                        {
                            if (type == LineType.Main || type == LineType.Function || type == LineType.Add)
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

                case LineType.Vars:
                    for (int i = destinationLine - 1; i >= 0; i--)
                    {
                        InterfazLinea lineToCheck = gEstatutos.Children[i] as InterfazLinea;
                        LineType type = lineToCheck.Type;

                        if (lineToCheck.LineNo == sourceLine) type = LineType.None;


                        if (type != LineType.None)
                        {
                            if (type == LineType.Main || type == LineType.Function || type == LineType.Vars || type == LineType.Program)
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

                case LineType.Other:
                case LineType.Main:
                    if (sourceLine > destinationLine)
                    {
                        for (int i = sourceLine - 1; i > destinationLine; i--)
                        {
                            InterfazLinea linetocheck = gEstatutos.Children[i] as InterfazLinea;

                            if (linetocheck.Type != LineType.None)
                            {
                                check = false;
                                break;
                            }

                        }
                    }
                    else
                    {
                        for (int i = sourceLine + 1; i < destinationLine; i++)
                        {
                            InterfazLinea linetocheck = gEstatutos.Children[i] as InterfazLinea;

                            if (linetocheck.Type != LineType.None)
                            {
                                check = false;
                                break;
                            }
                        }
                    }

                    break;


                
            }

            return check;
        }

        private void removeLine (int lineNo)
        {

            InterfazLinea intLinea = gEstatutos.Children[lineNo] as InterfazLinea;

            if (intLinea.Type == LineType.While || intLinea.Type == LineType.For || intLinea.Type == LineType.If || intLinea.Type == LineType.Else)
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

                    if (i != intLinea.LinkedTo)
                    {
                        intLineaAux.IndentLevel = intLinea.IndentLevel;
                    }

                }
            }
            else if (intLinea.Type == LineType.Main)
            {
                for (int i = intLinea.LineNo + 1; i < intLinea.LinkedTo; i++)
                {
                    InterfazLinea intLineaAux = ((InterfazLinea)gEstatutos.Children[i]);
                    intLineaAux.IndentLevel--;                             
                }              
            }


            intLinea.Type = LineType.None;
        }

        private void changeIndentation (InterfazLinea source, InterfazLinea destination)
        {
            if (source.LineNo > destination.LineNo)
            {
                for (int i = source.LineNo - 1; i >= destination.LineNo; i--)
                {
                    ((InterfazLinea)gEstatutos.Children[i]).IndentLevel--;
                }
            }
            else
            {
                for (int i = source.LineNo; i < destination.LineNo; i++)
                {
                    ((InterfazLinea)gEstatutos.Children[i]).IndentLevel++;
                }

            }
        }

        private void copyLine(InterfazLinea source, int destLineNo, bool copyIndent)
        {
            InterfazLinea destination = gEstatutos.Children[destLineNo] as InterfazLinea;

            destination.Type = source.Type;
            destination.Text = source.Text;
            destination.IsText = source.IsText;
            destination.LinkedTo = source.LinkedTo;

            if (destination.LinkedTo != - 1) (gEstatutos.Children[destination.LinkedTo] as InterfazLinea).LinkedTo = destination.LineNo;

            if (copyIndent)
            {
                destination.IndentLevel = source.IndentLevel;
                if (destination.Type == LineType.Other && destination.Text == "}")
                {
                    source.IndentLevel++;
                }
            }

            int i = 0;
            
            if (!(source.Actual is Image))
            {
                if (source.Actual != null)
                {
                    foreach (UIElement childSrc in ((StackPanel)source.Actual).Children)
                    {
                        UIElement childDest = ((StackPanel)destination.Actual).Children[i] as UIElement;

                        childDest.Visibility = childSrc.Visibility;

                        if (childSrc is TextBox)
                        {
                            ((TextBox)childDest).Text = ((TextBox)childSrc).Text;
                        }
                        else if (childSrc is ComboBox)
                        {
                            ((ComboBox)childDest).SelectedIndex = ((ComboBox)childSrc).SelectedIndex;
                        }

                        i++;

                    }
                }
            }


        }

        private void moveEverythingDown(int startLine)
        {
            addLines(1);
            int numLines = gEstatutos.Children.Count;

            for (int i = gEstatutos.Children.Count - 2; i > startLine; i--)
            {
                copyLine(gEstatutos.Children[i - 1] as InterfazLinea, i, true);
                ((InterfazLinea)gEstatutos.Children[i - 1]).Type = LineType.None;

            }


        }

        private void addLines (int numLines)
        {
            InterfazLinea oldReng = ((InterfazLinea)gEstatutos.Children[gEstatutos.Children.Count - 1]);
            oldReng.Type = LineType.None;
            oldReng.AddClicked -= AgregarPagina;

            InterfazLinea newReng = null;

            for (int i = 0; i < numLines; i++)
            {
                newReng = new InterfazLinea();
                newReng.Height = 29;
                newReng.SelectionChanged += FuncTypeSelectionChanged;
                newReng.LineNo = InterfazLinea.LastLineNo;
                newReng.Style = TryFindResource("EventsInterfazLinea") as Style;

                InterfazLinea.LastLineNo++;

                gEstatutos.Children.Add(newReng);
            }

            newReng.Type = LineType.Add;
            newReng.AddClicked += AgregarPagina;
        }

        private void processEst (InterfazLinea intLinea, bool move)
        {
            switch (intLinea.Type)
            {
                case LineType.If:

                    int i;
                    for (i = 1; i <= 3; i++)
                    {
                        InterfazLinea line = gEstatutos.Children[intLinea.LineNo + i] as InterfazLinea;

                        if (line.Type != LineType.None)
                        {
                            moveEverythingDown(line.LineNo);
                        }

                        if (i < 3) line.IndentLevel++;

                    }

                    i--;
                    InterfazLinea corchCi = gEstatutos.Children[intLinea.LineNo + i] as InterfazLinea;
                    corchCi.LinkedTo = intLinea.LineNo;
                    corchCi.Type = LineType.Other;
                    corchCi.IsText = true;
                    corchCi.Text = "}";
                    intLinea.LinkedTo = corchCi.LineNo;
                    if (!move)
                    {
                        i++;
                        InterfazLinea elseEst = gEstatutos.Children[intLinea.LineNo + i] as InterfazLinea;

                        if (elseEst.Type != LineType.None)
                        {
                            moveEverythingDown(elseEst.LineNo);
                        }

                        elseEst.Type = LineType.Else;
                        elseEst.IsText = true;
                        elseEst.Text = "else {";


                        for (i = 1; i <= 3; i++)
                        {
                            InterfazLinea line = gEstatutos.Children[elseEst.LineNo + i] as InterfazLinea;

                            if (line.Type != LineType.None)
                            {
                                moveEverythingDown(line.LineNo);
                            }

                            if (i < 3) line.IndentLevel++;
                        }

                        i--;
                        InterfazLinea corchCi2 = gEstatutos.Children[elseEst.LineNo + i] as InterfazLinea;
                        corchCi2.LinkedTo = elseEst.LineNo;
                        corchCi2.Type = LineType.Other;
                        corchCi2.IsText = true;
                        corchCi2.Text = "}";
                        elseEst.LinkedTo = corchCi2.LineNo;
                    }



                    break;

                case LineType.While:
                case LineType.For:
                case LineType.Function:
                case LineType.Else:
                    int j;
                    for (j = 1; j <= 4; j++)
                    {
                        InterfazLinea line = gEstatutos.Children[intLinea.LineNo + j] as InterfazLinea;

                        if (line.Type != LineType.None)
                        {
                            moveEverythingDown(line.LineNo);
                        }

                        if (j < 4) line.IndentLevel++;

                    }

                    j--;
                    InterfazLinea corchCi3 = gEstatutos.Children[intLinea.LineNo + j] as InterfazLinea;
                    corchCi3.LinkedTo = intLinea.LineNo;
                    corchCi3.Type = LineType.Other;
                    corchCi3.IsText = true;
                    corchCi3.Text = "}";
                    intLinea.LinkedTo = corchCi3.LineNo;
                    break;


                case LineType.Main:
                    for (i = intLinea.LineNo + 1; i < intLinea.LinkedTo; i++)
                    {
                        InterfazLinea line = gEstatutos.Children[i] as InterfazLinea;
                        line.IndentLevel++;
                    }

                    break;
            }
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

        private void FuncTypeSelectionChanged (object sender, EventArgs e)
        {
            InterfazLinea iLinea = sender as InterfazLinea;
            ComboBox cb = iLinea.funcType;

            if ((string)cb.SelectedItem == "void")
            {
                for (int i = iLinea.LineNo + 1; i < iLinea.LinkedTo; i++)
                {
                    if (i < gEstatutos.Children.Count)
                    {
                        InterfazLinea line = gEstatutos.Children[i] as InterfazLinea;

                        if (line.Type == LineType.Return) line.Type = LineType.None;
                    }

                }
            }
            else
            {

                bool hasReturn = false;

                
                for (int i = iLinea.LineNo + 1; i < iLinea.LinkedTo; i++)
                {
                    if (i < gEstatutos.Children.Count)
                    {
                        InterfazLinea line = gEstatutos.Children[i] as InterfazLinea;

                        if (line.Type == LineType.Return) hasReturn = true;
                    }

                }

                if (!hasReturn)
                {
                    int link = iLinea.LinkedTo;

                    if (iLinea.LinkedTo - 1 < gEstatutos.Children.Count)
                    {
                        if (((InterfazLinea)gEstatutos.Children[iLinea.LinkedTo - 1]).Type != LineType.None)
                        {
                            moveEverythingDown(iLinea.LinkedTo);
                            ((InterfazLinea)gEstatutos.Children[link]).Type = LineType.Return;
                        }
                        else
                        {
                            ((InterfazLinea)gEstatutos.Children[iLinea.LinkedTo - 1]).Type = LineType.Return;
                        }
                    }
                }

                
            }

        }
       

        private void imTrashcan_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("IntLinea"))
            {
                InterfazLinea intLinea = e.Data.GetData("IntLinea") as InterfazLinea;
                if (intLinea.Type != LineType.Main && intLinea.Type != LineType.Program && intLinea.Type != LineType.Other)
                {
                    removeLine(intLinea.LineNo);
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

        private string convertToString ()
        {
            string s = "";
            foreach (InterfazLinea intLinea in gEstatutos.Children)
            {
                if (intLinea.Type != LineType.None && intLinea.Type != LineType.Add)
                {
                    if (!intLinea.IsText)
                    {
                        intLinea.convertToText();
                    }

                    s += intLinea.Text + "\n";
                }
                else
                {
                    s += "\n";
                }

            }

            return s;
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Compile();
        }

        private List<string> getInput()
        {
            List<string> inputString = new List<string>();
            int lineCount = input.LineCount;

            for (int i = 0; i < lineCount; i++)
            {
                string line = input.GetLineText(i);
                var lineInput = Regex.Matches(line, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

                foreach (string st in lineInput)
                {
                    inputString.Add(Regex.Replace(st, @"\t|\n|\r", ""));
                }
            }

            inputString.RemoveAll(string.IsNullOrEmpty);
            return inputString;
        }

        private void Compile()
        {
            string pString;

            pString = convertToString();

            using (Stream s = GenerateStreamFromString(pString))
            {
                if (lineWithError != -1) ((InterfazLinea)gEstatutos.Children[lineWithError]).ErrorText = "";

                output.Text = "";

                Scanner scanner = new Scanner(s);
                Parser parser = new Parser(scanner);
                Programa p = null;

                List<string> inputString = getInput();

                try
                {
                    p = parser.Parse();
                    output.Text = "Sin errores de compilación. \n--\n";
                    VirtualMachine vm = new VirtualMachine(p, output, inputString);
                    vm.Run();
                    lineWithError = -1;
                }
                catch (FatalError ex)
                {
                    string[] msg = ex.Message.Split('*');
                    int lineErr = int.Parse(msg[0]);
                    ((InterfazLinea)gEstatutos.Children[lineErr - 1]).ErrorText = msg[1];
                    lineWithError = lineErr - 1;
                   
                }


                VirtualStructure.Reset();   
              
            }
        }

        private void Save (string fileDir)
        {
            using (StreamWriter write = new StreamWriter(fileDir))
            {
                for (int i = 0; i < gEstatutos.Children.Count - 1; i++)
                {
                    InterfazLinea toWrite = gEstatutos.Children[i] as InterfazLinea;
                    write.WriteLine(toWrite.ToString());
                }
            }

        }

        private void Open (string fileDir)
        {


            for (int i = 0; i < gEstatutos.Children.Count - 1; i++)
            {
                InterfazLinea toDelete = gEstatutos.Children[i] as InterfazLinea;
                toDelete.Type = LineType.None;
            }

            string line;
            using (StreamReader read = new StreamReader(fileDir))
            {
                int i = 0;
                while ((line = read.ReadLine()) != null)
                {

                    if (i == gEstatutos.Children.Count - 1) addLines(1);

                    InterfazLinea toWrite = gEstatutos.Children[i] as InterfazLinea;
                    toWrite.fromString(line);
                    i++;

                    

                }
            }
        }

        private void pIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Compile();
        }

        private void svIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            InterfazLinea pIntLinea = gEstatutos.Children[0] as InterfazLinea;
            string name = pIntLinea.tbProgram.Text;

            dlg.FileName = name; // Default file name
            dlg.DefaultExt = ".ple"; // Default file extension
            dlg.Filter = "PLearning Files (.ple)|*.ple"; // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                Save(filename);

                MessageBox.Show("Archivo guardado.");
                
            }
        }

        private void opIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".ple";
            dlg.Filter = "PLearning Files (.ple)|*.ple";


            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {  
                // Open document 
                string filename = dlg.FileName;
                Open(filename);
            }
        }
    }
}
