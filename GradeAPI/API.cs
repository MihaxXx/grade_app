﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using System.Web;

namespace Grade
{
    public class API
    {

        const string Host = @"grade";
        const string PathBase = @"~dev_rating/api/v1/";
        string Token;
        static readonly HttpClient client = new HttpClient();

        public API(string token)
		{
			Token = token;
		}


        public StudentIndex StudentGetIndex(int SemesterID)
        {
			var newuriB = new UriBuilder("http", Host)
			{
				Path = PathBase + "student"
			};
			var query = HttpUtility.ParseQueryString(string.Empty);
            query.Set("token", Token);
            query.Set("SemesterID", SemesterID.ToString());
            newuriB.Query = query.ToString();
            var Uri = newuriB.Uri;

            string response = string.Empty;
            try
            {
                response = client.GetStringAsync(Uri).Result;
            }
            catch (HttpRequestException)
            {

            }
            return StudentIndexRepsonse.FromJson(response).Response;
        }
        public StudentIndex StudentGetIndex()
        {
			var newuriB = new UriBuilder("http", Host)
			{
				Path = PathBase + "student"
			};
			var query = HttpUtility.ParseQueryString(string.Empty);
            query.Set("token", Token);
            newuriB.Query = query.ToString();
            var Uri = newuriB.Uri;

            string response = string.Empty;
            try
            {
                response = client.GetStringAsync(Uri).Result;
            }
            catch (HttpRequestException)
            {
               
            }
            return StudentIndexRepsonse.FromJson(response).Response;
		}
        public StudentDiscipline StudentGetDiscipline(long ID)
        {
			var newuriB = new UriBuilder("http", Host)
			{
				Path = PathBase + "student/discipline/subject"
			};
			var query = HttpUtility.ParseQueryString(string.Empty);
            query.Set("token", Token);
            query.Set("id", ID.ToString());
            newuriB.Query = query.ToString();
            var Uri = newuriB.Uri;

            string response = string.Empty;
            try
            {
                response = client.GetStringAsync(Uri).Result;
            }
            catch (HttpRequestException)
            {

            }
            return StudentDisciplineResponse.FromJson(response).Response;
        }
    }

    public partial class StudentIndexRepsonse
    {
        [JsonProperty("response")]
        public StudentIndex Response { get; set; }
    }

    public partial class StudentIndex
    {
        [JsonProperty("Marks")]
        public Dictionary<string, string> Marks { get; set; }

        [JsonProperty("Disciplines")]
        public Discipline[] Disciplines { get; set; }

        [JsonProperty("Teachers")]
        public Dictionary<string, TeacherValue> Teachers { get; set; }

        [JsonProperty("EMailChanged")]
        public bool EMailChanged { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}
	}

    public partial class StudentDisciplineResponse
    {
        [JsonProperty("response")]
        public StudentDiscipline Response { get; set; }
    }

    public partial class StudentDiscipline
    {
        [JsonProperty("Discipline")]
        public Discipline Discipline { get; set; }

        [JsonProperty("Teachers")]
        public Teacher[] Teachers { get; set; }

        [JsonProperty("DisciplineMap")]
        public DisciplineMap DisciplineMap { get; set; }

        [JsonProperty("Submodules")]
        public Dictionary<string, Submodule> Submodules { get; set; }

        [JsonProperty("ExtraRate")]
        public int ExtraRate { get; set; }

        [JsonProperty("Semester")]
        public Semester Semester { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public partial class Discipline
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("SubjectID")]
        public long SubjectId { get; set; }

        [JsonProperty("SubjectName")]
        public string SubjectName { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Subtype")]
        public object Subtype { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("SecondName")]
        public string SecondName { get; set; }

        [JsonProperty("Rate")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Rate { get; set; }

        [JsonProperty("MaxCurrentRate")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long MaxCurrentRate { get; set; }

        [JsonProperty("AuthorID")]
        public long AuthorId { get; set; }

        [JsonProperty("GradeID")]
        public long? GradeId { get; set; }

        [JsonProperty("GradeNum")]
        public long? GradeNum { get; set; }

        [JsonProperty("Degree")]
        public string Degree { get; set; }

        [JsonProperty("Lectures")]
        public int Lectures { get; set; }

        [JsonProperty("Practice")]
        public int Practice { get; set; }

        [JsonProperty("Labs")]
        public int Labs { get; set; }

        [JsonProperty("SemesterID")]
        public long SemesterId { get; set; }

        [JsonProperty("SubjectAbbr")]
        public string SubjectAbbr { get; set; }

        [JsonProperty("FacultyID")]
        public long FacultyId { get; set; }

        [JsonProperty("FacultyName")]
        public string FacultyName { get; set; }

        [JsonProperty("IsLocked")]
        public long IsLocked { get; set; }

        [JsonProperty("Milestone")]
        public long Milestone { get; set; }

        [JsonProperty("CompoundDiscID")]
        public object CompoundDiscId { get; set; }

        [JsonProperty("IsMapCreated")]
        public bool IsMapCreated { get; set; }

        [JsonProperty("IsGlobal")]
        public bool IsGlobal { get; set; }

        [JsonProperty("IsGlobalStub")]
        public bool IsGlobalStub { get; set; }

        [JsonProperty("IsBonus")]
        public bool IsBonus { get; set; }

        [JsonProperty("semesterNum")]
        public long SemesterNum { get; set; }

        [JsonProperty("semesterYear")]
        public long SemesterYear { get; set; }

        [JsonProperty("IsInactive")]
        public bool IsInactive { get; set; }

        [JsonProperty("GlobalName")]
        public string GlobalName { get; set; }
    }

    public partial class TeacherElement
    {
        [JsonProperty("DisciplineID")]
        public long DisciplineId { get; set; }

        [JsonProperty("TeacherID")]
        public long TeacherId { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("SecondName")]
        public string SecondName { get; set; }
    }

    public partial class DisciplineMap
    {
        [JsonProperty("Exam")]
        public long Exam { get; set; }

        [JsonProperty("Bonus")]
        public long Bonus { get; set; }

        [JsonProperty("Modules")]
        public Dictionary<string, Module> Modules { get; set; }
    }

    public partial class Module
    {
        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Submodules")]
        public long[] Submodules { get; set; }
    }

    public partial class Semester
    {
    }

    public partial class Submodule
    {
        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("MaxRate")]
        public int? MaxRate { get; set; }

        [JsonProperty("Rate")]
        public int? Rate { get; set; }

        [JsonProperty("Date")]
        public DateTime? Date { get; set; }
    }

    public partial class Teacher
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("SecondName")]
        public string SecondName { get; set; }

        [JsonProperty("JobPositionID")]
        public long JobPositionId { get; set; }

        [JsonProperty("JobPositionName")]
        public string JobPositionName { get; set; }

        [JsonProperty("DepID")]
        public object DepId { get; set; }

        [JsonProperty("DepName")]
        public object DepName { get; set; }

        [JsonProperty("FacultyID")]
        public long FacultyId { get; set; }

        [JsonProperty("FacultyAbbr")]
        public string FacultyAbbr { get; set; }

        [JsonProperty("IsAuthor")]
        public bool IsAuthor { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    public partial struct TeacherValue
    {
        public TeacherElement[] TeacherElementArray;
        public Dictionary<string, TeacherElement> TeacherElementMap;

        public static implicit operator TeacherValue(TeacherElement[] TeacherElementArray) => new TeacherValue { TeacherElementArray = TeacherElementArray };
        public static implicit operator TeacherValue(Dictionary<string, TeacherElement> TeacherElementMap) => new TeacherValue { TeacherElementMap = TeacherElementMap };
    }

    public partial class StudentIndexRepsonse
    {
        public static StudentIndexRepsonse FromJson(string json) => JsonConvert.DeserializeObject<StudentIndexRepsonse>(json, Grade.Converter.Settings);
    }

    public partial class StudentDisciplineResponse
    {
        public static StudentDisciplineResponse FromJson(string json) => JsonConvert.DeserializeObject<StudentDisciplineResponse>(json, Grade.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this StudentIndexRepsonse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this StudentDisciplineResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                TeacherValueConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class TeacherValueConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TeacherValue) || t == typeof(TeacherValue?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    var objectValue = serializer.Deserialize<Dictionary<string, TeacherElement>>(reader);
                    return new TeacherValue { TeacherElementMap = objectValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<TeacherElement[]>(reader);
                    return new TeacherValue { TeacherElementArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type TeacherValue");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (TeacherValue)untypedValue;
            if (value.TeacherElementArray != null)
            {
                serializer.Serialize(writer, value.TeacherElementArray);
                return;
            }
            if (value.TeacherElementMap != null)
            {
                serializer.Serialize(writer, value.TeacherElementMap);
                return;
            }
            throw new Exception("Cannot marshal type TeacherValue");
        }

        public static readonly TeacherValueConverter Singleton = new TeacherValueConverter();
    }
}
