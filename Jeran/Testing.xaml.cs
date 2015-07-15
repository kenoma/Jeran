using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using System.Threading;

namespace Jeran
{
    /// <summary>
    /// Interaction logic for Testing.xaml
    /// </summary>
    public partial class Testing : Window
    {
        
        private string wfolder { get; set; }
        private string Stage { get; set; }
        private int Position { get; set; }
        private string[] SlidesForStage { get; set; }
        private List<DilemmaFromResource> Questions { get; set; }
        private DilemmaAnswer Answer { get; set; }

        Thread th;
        DateTime clicktime;

        public DilemmaRespondent Respondent
        {
            get { return (DilemmaRespondent)GetValue(respProperty); }
            set { SetValue(respProperty, value); }
        }

        // Using a DependencyProperty as the backing store for resp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty respProperty =
            DependencyProperty.Register("resp", typeof(DilemmaRespondent), typeof(Testing));



        public TimeSpan delay
        {
            get { return (TimeSpan)GetValue(delayProperty); }
            set { SetValue(delayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for delay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty delayProperty =
            DependencyProperty.Register("delay", typeof(TimeSpan), typeof(Testing));

        

        public Testing(DilemmaRespondent respondent, string workfolder)
        {
            InitializeComponent();
            Questions = new List<DilemmaFromResource>();
            Respondent = respondent;
            wfolder = workfolder;
            LoadForm(wfolder + "\\b-0-Hello.xml");
            Stage = "b";
            Position = 0;
            SlidesForStage = Directory.GetFiles(wfolder, "b-*.xml");
            Answer = null;

            th  = new Thread(() => { while (true) { CheckStatus(); Thread.Sleep(1000); } });
            th.Start();
        }

        public void CheckStatus()
        {
            TimeSpan sp = DateTime.Now - clicktime;

            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                delay = Properties.Settings.Default.MinDelay - sp;
                if (delay.Seconds > 0)
                {
                    timer.Visibility = System.Windows.Visibility.Visible;
                    nextbutton.IsEnabled = false;
                }
                else
                {
                    nextbutton.IsEnabled = true;
                    timer.Visibility = System.Windows.Visibility.Hidden;
                }
            }));
        }

        private void nextbutton_Click(object sender, RoutedEventArgs e)
        {
            display.DataContext = null;
            display.DataContext = Respondent;

            if (!Validator.IsValid(display))
            {
                Console.WriteLine("Not valid");
                return;
            }

            clicktime = DateTime.Now;
            timer.Visibility = System.Windows.Visibility.Visible;
            nextbutton.IsEnabled = false;

            Respondent.HR1 = Respondent.HR2 = false;

            if (Stage.Equals("b"))
                ProceedPrephase();

            if (Stage.Equals("q"))
                ProceedQuestions();

            if (Stage.Equals("e"))
                ProceedEnding();
        }

        private void ProceedEnding()
        {
            if (Position == 0 && Position < SlidesForStage.Length)
                LoadForm(SlidesForStage[0]);
            else
            {
                Respondent.Date = DateTime.Now;
                if (!Directory.Exists("Data"))
                    Directory.CreateDirectory("Data");
                ser<DilemmaRespondent>.SerializefzProtoBuf(Respondent, "Data\\resp-" + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".bin");
                Close();
            }

            Position++;
        }

        private void ProceedPrephase()
        {
            Position++;
            string slide = SlidesForStage.FirstOrDefault(x => x.StartsWith(string.Format("{0}\\{1}-{2}-", wfolder, Stage, Position)));
            if (Stage.Equals("b") && string.IsNullOrEmpty(slide))
            {
                Stage = "q";
                Position = 0;
                SlidesForStage = Directory.GetFiles(wfolder, "q-*.xml");
                Random rnd = new Random();


                object obj = Properties.Resources.ResourceManager.GetObject("dilemma_1");
                int i = 1;
                while (obj != null)
                {
                    string[] parts = obj.ToString().Split(';');
                    Questions.Add(new DilemmaFromResource() { Question = parts[2], Text = parts[1], ID = i++, Title = parts[0] });
                    obj = Properties.Resources.ResourceManager.GetObject("dilemma_" + i);
                }
                Questions = new List<DilemmaFromResource>(Questions.OrderBy(z => rnd.NextDouble()));

            }
            else
                LoadForm(slide);
        }

        private void ProceedQuestions()
        {
            if (Answer != null)
            {
                for (int i = 0; i < Respondent.RadioButtonHelp.Length; i++)
                    if (Respondent.RadioButtonHelp[i])
                        Answer.Answer = i + 1;
                Answer.HeartBeatEnd = Respondent.RR.Count - 1;
                Respondent.DilemmaAnswers.Add(Answer);
            }

            if (Position >= Questions.Count)
            {
                Stage = "e";
                Position = 0;
                SlidesForStage = Directory.GetFiles(wfolder, "e-*.xml");
            }
            else
            {
                Respondent.DTitle = Questions[Position].Title;
                Respondent.DDilemmaText = Questions[Position].Text;
                Respondent.DDilemaQuestion = Questions[Position].Question;

                for (int i = 0; i < 7; i++)
                    Respondent.RadioButtonHelp[i] = false;
                LoadForm(SlidesForStage[0]);
                Answer = new DilemmaAnswer() { TimeStart = DateTime.Now, HeartBeatStart = Respondent.RR.Count - 1, Dilemma = Questions[Position].ID };
                Position++;
              
            }
        }



        private void LoadForm(string form)
        {
            StreamReader mysr = new StreamReader(form);
            FrameworkElement rootObject = XamlReader.Load(mysr.BaseStream) as FrameworkElement;
            for (int radioButtons = 0; radioButtons < 7; radioButtons++)
            {
                RadioButton rButton = LogicalTreeHelper.FindLogicalNode(rootObject, "R" + radioButtons) as RadioButton;
                if (rButton != null)
                {
                    rButton.Click += UpdateValidation;
                }
            }


            display.Children.Clear();
            display.Children.Add(rootObject);

            rootObject.BeginInit();
            display.DataContext = Respondent;

            //rootObject.Measure(renderingSize);
            //rootObject.Arrange(renderingRectangle);

            rootObject.EndInit();
            rootObject.UpdateLayout();
            
            
        }

        private T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void testw_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (th != null && th.IsAlive)
            {
                th.Abort();
                Console.WriteLine("Delay timer killed");
            }
        }

        private void UpdateValidation(object sender, RoutedEventArgs e)
        {
            display.DataContext = null;
            display.DataContext = Respondent;
        }
    }

    internal class DilemmaFromResource
    {
        public string Title { get; set; }
        public string Text{ get; set; }
        public string Question { get; set; }
        public int ID { get; set; }
    }

}
