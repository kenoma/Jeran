using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using System.IO;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Jeran
{
    [ProtoContract, Serializable]
    public class DilemmaAnswer
    {
        [ProtoMember(1)]
        public int Dilemma { get; set; }
        [ProtoMember(2)]
        public int Answer { get; set; }
        [ProtoMember(3)]
        public int HeartBeatStart { get; set; }
        [ProtoMember(4)]
        public int HeartBeatEnd { get; set; }
        [ProtoMember(5)]
        public DateTime TimeStart { get; set; }

        [NonSerialized]
        public double _elapsed = 0;
    }

    [ProtoContract, Serializable]
    public class DilemmaRespondent : IDataErrorInfo, INotifyPropertyChanged
    {
        [ProtoMember(1)]
        public long ID { get; set; }
        [ProtoMember(2)]
        public DateTime Date { get; set; }
        [ProtoMember(3)]
        public int Age { get; set; }
        [ProtoMember(4)]
        public string Gender { get; set; }
        [ProtoMember(5)]
        public string Ethnicity { get; set; }
        [ProtoMember(6)]
        public string Education { get; set; }
        [ProtoMember(7)]
        public string Occupation { get; set; }
        [ProtoMember(8)]
        public string CountryLive { get; set; }
        [ProtoMember(9)]
        public string CountryChildhood { get; set; }
        [ProtoMember(10)]
        public string CountryIdentify { get; set; }
        [ProtoMember(11)]
        public string Language { get; set; }
        [ProtoMember(12)]
        public string BirthOrder { get; set; }
        [ProtoMember(13)]
        public bool Married { get; set; }
        [ProtoMember(14)]
        public int Children { get; set; }
        [ProtoMember(15)]
        public bool MoralCourses { get; set; }
        [ProtoMember(16)]
        public string Books { get; set; }
        [ProtoMember(17)]
        public string ReligionGrow { get; set; }
        [ProtoMember(18)]
        public string ReligionIdentify { get; set; }
        [ProtoMember(19)]
        public string ReligionLevel { get; set; }
        [ProtoMember(20)]
        public List<double> RR { get; set; }
        [ProtoMember(21)]
        public List<DilemmaAnswer> DilemmaAnswers { get; set; }

        [field: NonSerialized]
        public string fileName { get; set; }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        

        [NonSerialized]
        public bool[] RadioButtonHelp = new bool[7];

        public string DTitle { get; set; }
        public string DDilemmaText { get; set; }
        public string DDilemaQuestion { get; set; }

        
        public bool R1 { get { return RadioButtonHelp[0]; } set { RadioButtonHelp[0] = value; } }
        
        public bool R2 { get { return RadioButtonHelp[1]; } set { RadioButtonHelp[1] = value; } }
        
        public bool R3 { get { return RadioButtonHelp[2]; } set { RadioButtonHelp[2] = value; } }
        
        public bool R4 { get { return RadioButtonHelp[3]; } set { RadioButtonHelp[3] = value; } }
        
        public bool R5 { get { return RadioButtonHelp[4]; } set { RadioButtonHelp[4] = value; } }
        
        public bool R6 { get { return RadioButtonHelp[5]; } set { RadioButtonHelp[5] = value; } }
        
        public bool R7 { get { return RadioButtonHelp[6]; } set { RadioButtonHelp[6] = value; } }
        
        public bool HR1 { get; set; }
        
        public bool HR2 { get; set; }

        
        public string Error
        {
            get;
            private set;
        }
        
        public string this[string columnName]
        {
            get
            {
                string result = null;

                switch (columnName)
                {
                    case "Gender":
                        if (string.IsNullOrEmpty(Gender))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;
                    case "Ethnicity":
                        if (string.IsNullOrEmpty(Ethnicity))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "Education":
                        if (string.IsNullOrEmpty(Education))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "Occupation":
                        if (string.IsNullOrEmpty(Occupation))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;
                    case "CountryLive":
                        if (string.IsNullOrEmpty(CountryLive))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "CountryChildhood":
                        if (string.IsNullOrEmpty(CountryChildhood) && HR2)
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "CountryIdentify":
                        if (string.IsNullOrEmpty(CountryIdentify))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;
                    case "Language":
                        if (string.IsNullOrEmpty(Language))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "BirthOrder":
                        if (string.IsNullOrEmpty(BirthOrder))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "Married":
                        if (!Married && !HR1)
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "Children":
                        if (Children < 0 || Children >= 20)
                            result = Properties.Resources.ErrorTextInputValidation;
                        break;

                    case "MoralCourses":
                        if (!MoralCourses && !HR2)
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "Books":
                        if (string.IsNullOrEmpty(Books))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "ReligionGrow":
                        if (string.IsNullOrEmpty(ReligionGrow))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "ReligionIdentify":
                        if (string.IsNullOrEmpty(ReligionIdentify))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "ReligionLevel":
                        if (string.IsNullOrEmpty(ReligionLevel))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "Age":
                        if (Age <= 2 || Age >= 130)
                            result = Properties.Resources.ErrorTextInputValidation;
                        break;

                    case "HR1":
                        if (!(HR1 || Married) && !HR2)
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "HR2":
                        if (!HR1 && !(HR2 || MoralCourses))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "R1":
                        if (RadioButtonHelp.All(z => !z))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "R2":
                        if (RadioButtonHelp.All(z => !z))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "R3":
                        if (RadioButtonHelp.All(z => !z))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "R4":
                        if (RadioButtonHelp.All(z => !z))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "R5":
                        if (RadioButtonHelp.All(z => !z))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "R6":
                        if (RadioButtonHelp.All(z => !z))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    case "R7":
                        if (RadioButtonHelp.All(z => !z))
                            result = Properties.Resources.ErrorComboboxValidation;
                        break;

                    default:
                        Console.WriteLine(columnName);
                        break;
                }

                return result;
            }
        }

        private int heartrate=0;
        
        public int HeartRate
        {
            get
            {
                return heartrate;
            }
            set
            {
                heartrate = value;
                RaisePropertyChanged("HeartRate");
            }
        }

        private int charge = 0;
        
        public int Charge
        {
            get
            {
                return charge;
            }
            set
            {
                charge = value;
                RaisePropertyChanged("Charge");
            }
        }

        public DilemmaRespondent()
        {
            RR = new List<double>();
            DilemmaAnswers = new List<DilemmaAnswer>();

            for (int i = 0; i < 7; i++)
                RadioButtonHelp[i] = false;
            Children = -1;
        }



        private void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
            {
                e(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    public static class ser<T>
    {
        public static void SerializefzProtoBuf(T pObject, string filename)
        {
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
                    ProtoBuf.Serializer.Serialize(fs, pObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Stream stream = File.Open(filename+"_", FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();

                Console.WriteLine("Writing Employee Information");
                if (pObject != null)
                    bformatter.Serialize(stream, pObject);
                stream.Close();
                
            }
        }

        public static T DeserializefzProtobuf(string filename)
        {
            try
            {
                T tr;
                using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open))
                    tr = ProtoBuf.Serializer.Deserialize<T>(fs);
                return tr;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default(T);
            }
        }
    }

}
