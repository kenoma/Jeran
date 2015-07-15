using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ZedGraph;

namespace HxMSymbolicHeart
{
    public partial class OfflineRplot : Form
    {

        double[] RR = new double[0];
        string FName = "";
        Symbolicprocessor.FormRecMatrix rmatrix = Symbolicprocessor.RecurencePlot.Rmatrix;

        public OfflineRplot(double[] data)
        {
            InitializeComponent();
            RR = data;
            button2_Click(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Text Files (.csv)|*.csv|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            openFileDialog1.Multiselect = true;


            DialogResult userClickedOK = openFileDialog1.ShowDialog();

            if (userClickedOK == System.Windows.Forms.DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(openFileDialog1.FileName);
                if (lines.Length > 1)
                {
                    int RRHeader = 0;
                    string[] headers = lines[0].Split(',');
                    for (int i = 0; i < headers.Length; i++)
                        if (headers[i].Equals("RR"))
                        {
                            RRHeader = i;
                            break;
                        }


                    RR = lines.Skip(1).Select(x => double.Parse(x.Split(',')[RRHeader])).ToArray();
                    FName = openFileDialog1.FileName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (RR.Length != 0)
            {
                double[][] RMatrix = rmatrix(RR, 1, int.Parse(edimtb.Text));
                double eps = Symbolicprocessor.RecurencePlot.EstimateEpsByPercentile(RMatrix, double.Parse(epsedottb.Text));

                Dictionary<string,double> dic = new Dictionary<string,double>();
                Symbolicprocessor.RecurencePlot.Stats(RMatrix,eps,ref dic);

                summary.Text = FName + " Len:" + RR.Length + "\r\n" + string.Concat(dic.Select(x => string.Format("{0}:{1:0.00}\t", x.Key, x.Value)));

                Bitmap pl = new Bitmap(RR.Length, 100);
                double min = RR.Min();
                double max = RR.Max();
                using (var graphics = Graphics.FromImage(pl))
                {
                    for (int i = 1; i < RR.Length; i++)
                    {
                        graphics.DrawLine(new Pen(Brushes.Black), 
                            i - 1, Convert.ToInt32(pl.Height * (RR[i - 1] - min) / (max - min)), 
                            i, Convert.ToInt32(pl.Height * (RR[i] - min) / (max - min)));
                    }
                }
                plot.Image = pl;

                Bitmap bmp = new Bitmap(RMatrix.Length, RMatrix.Length);
                for (int i = 0; i < RMatrix.Length; i++)
                    for (int j = 0; j < RMatrix.Length; j++)
                        if (RMatrix[i][j] <= eps)
                            bmp.SetPixel(i, j, Color.Black);
                        else
                            bmp.SetPixel(i, j, Color.White);
                pictureBox1.Image = bmp;
                bmp.Save(FName + ".bmp");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (RR.Length != 0&& comboBox1.SelectedItem!=null)
            {
                GraphPane pane = zedGraphControl2.GraphPane;
                pane.CurveList.Clear();
                PointPairList ppl = new PointPairList();
                pane.AddCurve(comboBox1.SelectedItem.ToString(), ppl, Color.Black, SymbolType.None);
                int wlen = int.Parse(wlenedittb.Text);

                double[][] RMatrix = rmatrix(RR, 1, int.Parse(edimtb.Text));
                double eps = Symbolicprocessor.RecurencePlot.EstimateEpsByPercentile(RMatrix, double.Parse(epsedottb.Text));

                for (int t = 0; t < RR.Length - wlen; t++)
                {
                    double[] tmp = RR.Skip(t).Take(wlen).ToArray();
                    RMatrix = rmatrix(tmp, 1, int.Parse(edimtb.Text));
                    
                    Dictionary<string, double> dic = new Dictionary<string, double>();
                    Symbolicprocessor.RecurencePlot.Stats(RMatrix, eps, ref dic);

                    ppl.Add(t, dic[comboBox1.SelectedItem.ToString()]);
                }
                
                zedGraphControl2.AxisChange();
                zedGraphControl2.Invalidate();

                
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (RR.Length != 0)
            {

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Text Files (.csv)|*.csv|All Files (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.FileName = FName + "_postproc.csv";

                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    int wlen = int.Parse(wlenedittb.Text);
                    StringBuilder sb = new StringBuilder();

                    for (int t = 0; t < RR.Length - wlen; t++)
                    {

                        double[] tmp = RR.Skip(t).Take(wlen).ToArray();

                        double[][] RMatrix = rmatrix(tmp, 1, int.Parse(edimtb.Text));
                        double eps = Symbolicprocessor.RecurencePlot.EstimateEpsByPercentile(RMatrix, double.Parse(epsedottb.Text));

                        Dictionary<string, double> dic = new Dictionary<string, double>();
                        Symbolicprocessor.RecurencePlot.Stats(RMatrix, eps, ref dic);

                        if (sb.Length == 0)
                            sb.AppendLine(dic.Keys.Aggregate((x, y) => x + "," + y));

                        sb.AppendLine(dic.Values.Select(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)).Aggregate((x, y) => x + "," + y));

                    }
                    File.WriteAllText(saveDialog.FileName, sb.ToString());
                    MessageBox.Show("Done");
                }

            }
        }

        private void OfflineRplot_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedItem.ToString())
            {
                case "RR": rmatrix = Symbolicprocessor.RecurencePlot.Rmatrix; break;
                case "bb": rmatrix = Symbolicprocessor.RecurencePlot.Rmatrix_bb; break;
                case "RR_patterns": rmatrix = Symbolicprocessor.RecurencePlot.Rmatrix_patterns; break;
                case "kurth": rmatrix = Symbolicprocessor.RecurencePlot.Rmatrix_kurth; break;
            }
        }
    }
}
