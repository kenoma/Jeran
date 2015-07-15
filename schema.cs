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
	}
	
	//Класс для сериализации\десериализации данных
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