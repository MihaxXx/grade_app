using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Grade
{
    public enum Role { Student, Teacher };
    public class API
    {
        public readonly Role role;
#if DEV_RATING
        public const string Host = @"dev.rating.mmcs.sfedu.ru";
        public const string PathToAPI = @"~dev_rating/api/v1/";
#elif LOCAL
		public const string Host = @"192.168.88.16";
		public const string PathToAPI = @"~dev_rating/api/v1/";
#else
        public const string Host = @"grade.sfedu.ru";
        public const string PathToAPI = @"api/v1/";
#endif
        readonly string PathBase = PathToAPI;
        readonly string Token;

        static readonly string trustedThumbprint = "35889ED18D9C97DCE7ADF779F7BBF600CB687DDEB00B3854D25CD74D5D736678";

        static readonly HttpClient client = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
            {
                if (errors == System.Net.Security.SslPolicyErrors.None)
                    return true;

                if (errors == System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors &&
                cert?.GetCertHashString(System.Security.Cryptography.HashAlgorithmName.SHA256) == trustedThumbprint)
                {
                    return true;
                }

                return false;
            }
        })
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        public API(string token, Role _role)
        {
            Token = token;
            role = _role;
            PathBase += (_role == Role.Student ? "student" : "teacher") + "/";
        }

        public async Task<string> Request(Dictionary<string, string> args, string relPath)
        {
            var newuriB = new UriBuilder(
#if LOCAL
                "http"
#else
                "https"
#endif
                , Host)
            {
                Path = PathBase + relPath
            };
            var query = HttpUtility.ParseQueryString(string.Empty);
            query.Set("token", Token);
            foreach (var arg in args)
                query.Set(arg.Key, arg.Value);
            newuriB.Query = query.ToString();
            var Uri = newuriB.Uri;

            string response = string.Empty;
            try
            {
                response = await client.GetStringAsync(Uri);
            }
            catch (Exception e) when (e is HttpRequestException || /*e is TaskCanceledException ||*/ e is InvalidOperationException || e is UriFormatException || e is AggregateException)
            {
                // TODO: пробрасывать провалы запроса и исключения в функции, инициировавшие запрос
            }
            return response;
        }

        public (bool, string) Post(Dictionary<string, string> args, string relPath)
        {
            var newuriB = new UriBuilder(
#if LOCAL
                "http"
#else
                "https"
#endif
                , Host)
            {
                Path = PathBase + relPath
            };
            var query = HttpUtility.ParseQueryString(string.Empty);
            query.Set("token", Token);
            /*foreach (var arg in args)
                query.Set(arg.Key, arg.Value);*/
            newuriB.Query = query.ToString();
            var content = new FormUrlEncodedContent(args);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            /*content.Headers.*/
            var Uri = newuriB.Uri;

            bool success = false;
            string result = "";
            try
            {
                var response = client.PostAsync(Uri, content).Result;
                result = response.Content.ReadAsStringAsync().Result;
                success = response.IsSuccessStatusCode;
            }
            catch (Exception e) when (e is HttpRequestException || e is TaskCanceledException || e is InvalidOperationException || e is UriFormatException || e is AggregateException)
            {
                return (false, e.Message);
            }
            return (success, result);
        }

        public static (bool, string) PostNoUser(Dictionary<string, string> query_args, Dictionary<string, string> form_urlencoded_args, string relPath)
        {
            var newuriB = new UriBuilder(
#if LOCAL
				"http"
#else
                "https"
#endif
                , Host)
            {
                Path = PathToAPI + relPath
            };
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var arg in query_args)
                query.Set(arg.Key, arg.Value);
            newuriB.Query = query.ToString();
            var content = new FormUrlEncodedContent(form_urlencoded_args);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            /*content.Headers.*/
            var Uri = newuriB.Uri;

            bool success = false;
            string result = "";
            try
            {
                var response = client.PostAsync(Uri, content).Result;
                result = response.Content.ReadAsStringAsync().Result;
                success = response.IsSuccessStatusCode;
            }
            catch (Exception e) when (e is HttpRequestException || e is TaskCanceledException || e is InvalidOperationException || e is UriFormatException || e is AggregateException)
            {
                return (false, e.Message);
            }
            return (success, result);
        }
        public async Task<StudentIndex> StudentGetIndex(long SemesterID = -1)
        {
            var args = new Dictionary<string, string>();
            if (SemesterID != -1)
                args.Add("SemesterID", SemesterID.ToString());
            return StudentIndexResponse.FromJson(await Request(args, "")).Response;
        }

        public async Task<StudentDiscipline> StudentGetDiscipline(long ID)
        {
            var args = new Dictionary<string, string>
            {
                { "id", ID.ToString() }
            };
            return StudentDisciplineResponse.FromJson(await Request(args, "discipline/subject")).Response;
        }
        public async Task<List<Semester>> GetSemesterList()
        {
            var args = new Dictionary<string, string>();
            return SemesterListResponse.FromJson(await Request(args, "semester_list")).Response.Values.ToList();
        }
        public async Task<TeacherIndex> TeacherGetIndex(long SemesterID = -1)
        {
            var args = new Dictionary<string, string>();
            if (SemesterID != -1)
                args.Add("SemesterID", SemesterID.ToString());
            return TeacherIndexResponse.FromJson(await Request(args, "")).Response;
        }
        public async Task<TeacherDiscipline> TeacherGetDiscipline(long ID)
        {
            var args = new Dictionary<string, string>
            {
                { "id", ID.ToString() }
            };
            return TeacherDisciplineResponse.FromJson(await Request(args, "discipline/rating")).Response;
        }
        public async Task<StudentJournal> StudentGetDisciplineJournal(long ID)
        {
            var args = new Dictionary<string, string>
            {
                { "id", ID.ToString() }
            };
            return StudentJournalResponse.FromJson(await Request(args, "discipline/journal")).Response;
        }

        public async Task<TeacherJournal> TeacherGetDisciplineJournal(long ID)
        {
            var args = new Dictionary<string, string>
            {
                { "id", ID.ToString() }
            };
            return TeacherJournalResponse.FromJson(await Request(args, "discipline/journal")).Response;
        }

        public (bool, string) TeacherPostSetRate(long recordBookID, long disciplineID, long submoduleID, int rate)
        {
            var args = new Dictionary<string, string>
            {
                { nameof(recordBookID),recordBookID.ToString() },
                { nameof(disciplineID), disciplineID.ToString() },
                { nameof(submoduleID), submoduleID.ToString() },
                { nameof(rate), rate.ToString() }
            };
            var rawRes = Post(args, "set_rate");
            if (!rawRes.Item1)
                return rawRes;
            var res = PostRequestResponse.FromJson(rawRes.Item2).Response;
            return (res.Success, res.Message);
        }

        public (bool, string) TeacherPostSetAttendance(long lessonID, long recordBookID, bool attendance)
        {
            var args = new Dictionary<string, string>
            {
                { nameof(lessonID),lessonID.ToString() },
                { nameof(recordBookID), recordBookID.ToString() },
                { nameof(attendance), (attendance? 1 : 0).ToString() }
            };
            var rawRes = Post(args, "set_attendance");
            if (!rawRes.Item1)
                return rawRes;
            var res = PostRequestResponse.FromJson(rawRes.Item2).Response;
            return (res.Success, res.Message);
        }

        public (bool, string) TeacherPostCreateLesson(long disciplineID, DateTime date, LessonType lessonType, Subgroup lessonSubgroup = null)
        {
            var args = new Dictionary<string, string>
            {
                { nameof(disciplineID), disciplineID.ToString() },
                { nameof(date), date.ToString("yyyy-M-d") },
                { nameof(lessonType), lessonType.Id.ToString() }
            };
            if (lessonSubgroup != null)
                args.Add(nameof(lessonSubgroup), lessonSubgroup.Id.ToString());
            var rawRes = Post(args, "create_lesson");
            if (!rawRes.Item1)
                return rawRes;
            var res = PostRequestResponse.FromJson(rawRes.Item2).Response;
            return (res.Success, res.Message);
        }

        public (bool, string) TeacherPostDeleteLesson(long disciplineID, long lessonID)
        {
            var args = new Dictionary<string, string>
            {
                { nameof(disciplineID), disciplineID.ToString() },
                { nameof(lessonID), lessonID.ToString() }
            };
            var rawRes = Post(args, "delete_lesson");
            if (!rawRes.Item1)
                return rawRes;
            var res = PostRequestResponse.FromJson(rawRes.Item2).Response;
            return (res.Success, res.Message);
        }
        public static (bool, string) PostGetToken(string login, string password)
        {
            var args = new Dictionary<string, string>
            {
                { nameof(login), login },
                { nameof(password), password }
            };
            var rawRes = PostNoUser(new Dictionary<string, string>(), args, "auth/get_token");
            if (!rawRes.Item1)
                return rawRes;
            var res = GetTokenResponse.FromJson(rawRes.Item2).Response;
            return (res.Success, res.Success ? res.Token : res.Message);
        }
    }

    public partial class StudentIndexResponse
    {
        [JsonProperty("response")]
        public StudentIndex Response { get; set; }
    }

    public partial class StudentIndex
    {
        [JsonProperty("Marks")]
        public Dictionary<long, string> Marks { get; set; }

        [JsonProperty("Disciplines")]
        public Discipline[] Disciplines { get; set; }

        [JsonProperty("Teachers")]
        public Dictionary<long, TeacherValue> Teachers { get; set; }

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
        public Dictionary<long, Submodule> Submodules { get; set; }

        [JsonProperty("ExtraRate")]
        public int ExtraRate { get; set; }

        [JsonProperty("Semester")]
        public Semester Semester { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public partial class SemesterListResponse
    {
        [JsonProperty("response")]
        public Dictionary<string, Semester> Response { get; set; }
    }
    public partial class TeacherIndexResponse
    {
        [JsonProperty("response")]
        public TeacherIndex Response { get; set; }
    }

    public partial class TeacherIndex
    {
        [JsonProperty("Subjects")]
        public Dictionary<string, Subject> Subjects { get; set; }

        [JsonProperty("Teachers")]
        public Dictionary<string, Dictionary<string, Teacher>> Teachers { get; set; }

        [JsonProperty("Groups")]
        public Dictionary<string, string[]> Groups { get; set; }

        [JsonProperty("DisciplineCreationISAllowed")]
        public bool DisciplineCreationIsAllowed { get; set; }

        [JsonProperty("EMailChanged")]
        public bool EMailChanged { get; set; }
    }

    public partial class TeacherDisciplineResponse
    {
        [JsonProperty("response")]
        public TeacherDiscipline Response { get; set; }
    }

    public partial class TeacherDiscipline
    {
        [JsonProperty("Discipline")]
        public Discipline Discipline { get; set; }

        [JsonProperty("Modules")]
        public Dictionary<long, ModuleT> Modules { get; set; }

        [JsonProperty("Groups")]
        public Dictionary<long, Group> Groups { get; set; }

        [JsonProperty("Students")]
        public Dictionary<long, Student[]> Students { get; set; }

        //RecordBookID to<SubModule ID, rate>
        [JsonProperty("Rates")]
        public Dictionary<long, Dictionary<long, int>> Rates { get; set; }

        [JsonProperty("Exams")]
        public Dictionary<string, Dictionary<string, Exam>> Exams { get; set; }

        [JsonProperty("Milestone")]
        public Milestone Milestone { get; set; }

        [JsonProperty("IsAuthor")]
        public bool IsAuthor { get; set; }
    }

    public partial class StudentJournalResponse
    {
        [JsonProperty("response")]
        public StudentJournal Response { get; set; }
    }

    public partial class StudentJournal
    {
        [JsonProperty("Discipline")]
        public Discipline Discipline { get; set; }

        [JsonProperty("Teachers")]
        public Teacher[] Teachers { get; set; }

        [JsonProperty("Semester")]
        public Semester Semester { get; set; }

        [JsonProperty("Journal")]
        public Journal[] Journal { get; set; }

        [JsonProperty("GymAttendanceInfo")]
        public GymAttendanceInfo GymAttendanceInfo { get; set; }

        [JsonProperty("IsGym")]
        public bool IsGym { get; set; }
    }

    public partial class TeacherJournalResponse
    {
        [JsonProperty("response")]
        public TeacherJournal Response { get; set; }
    }

    public partial class TeacherJournal
    {
        [JsonProperty("Discipline")]
        public Discipline Discipline { get; set; }

        [JsonProperty("Lessons")]
        public Lesson[] Lessons { get; set; }

        [JsonProperty("Groups")]
        public Dictionary<long, Group> Groups { get; set; }

        [JsonProperty("Subgroups")]
        public Dictionary<long, Subgroup> Subgroups { get; set; }

        [JsonProperty("Students")]
        public Dictionary<long, Student[]> Students { get; set; }

        [JsonProperty("LessonTypes")]
        public LessonType[] LessonTypes { get; set; }

        [JsonProperty("Attendance")]
        public Dictionary<long, System.Collections.Generic.Dictionary<long, long>> Attendance { get; set; }
    }

    public partial class PostRequestResponse
    {
        [JsonProperty("response")]
        public PostRequest Response { get; set; }
    }

    public partial class PostRequest
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
    public partial class GetTokenResponse
    {
        [JsonProperty("response")]
        public GetToken Response { get; set; }
    }

    public partial class GetToken
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class Subject
    {
        [JsonProperty("SubjectName")]
        public string SubjectName { get; set; }

        [JsonProperty("GradeNum")]
        public long? GradeNum { get; set; }

        [JsonProperty("Degree")]
        public Degree Degree { get; set; }

        public string ShortDegree()
        {
            switch (this.Degree)
            {
                case Degree.Bachelor: return "бак";
                case Degree.Master: return "маг";
                case Degree.Postgraduate: return "асп";
                case Degree.Specialist: return "спец";
                default: return Degree.ToString();
            }
        }

        [JsonProperty("Disciplines")]
        public Discipline[] Disciplines { get; set; }
    }

    public partial class Semester
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("Year")]
        public int Year { get; set; }

        [JsonProperty("Num")]
        public int Num { get; set; }

        [JsonProperty("Season")]
        public string Season { get; set; }

        public override string ToString()
        {
            return $"{(Num == 1 ? "Осень" : "Весна")} {(Num == 1 ? Year : Year + 1)}";
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

        public string TypeToString() => Type switch
        {
            "exam" => "Экзамен",
            "credit" => "Зачет",
            "grading_credit" => "Дифф. зачет",
            _ => Type,
        };

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

        [JsonProperty("GroupID")]
        public long? GroupId { get; set; }

        [JsonProperty("GroupNum")]
        public int? GroupNum { get; set; }

        [JsonProperty("GroupName")]
        public string GroupName { get; set; }

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
        public long? CompoundDiscId { get; set; }

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

        [JsonProperty("GlobalID")]
        public string GlobalId { get; set; }

        [JsonProperty("IsFrozen")]
        public bool IsFrozen { get; set; }

        [JsonProperty("Frozen")]
        public bool Frozen { get; set; }

        [JsonProperty("AutoJournal")]
        public bool AutoJournal { get; set; }

        [JsonProperty("LocalizedExamType")]
        public string LocalizedExamType { get; set; }

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
        public Dictionary<long, Module> Modules { get; set; }
    }

    public partial class Module
    {
        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Submodules")]
        public long[] Submodules { get; set; }
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

    public partial class ModuleT
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Type")]
        public ModuleType Type { get; set; }

        [JsonProperty("Submodules")]
        public SubmoduleT[] Submodules { get; set; }

        [JsonProperty("MaxRate")]
        public long MaxRate { get; set; }
    }

    public partial class SubmoduleT
    {
        [JsonProperty("ModuleID")]
        public long ModuleId { get; set; }

        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Rate")]
        public int Rate { get; set; }

        [JsonProperty("Type")]
        public SubModuleType Type { get; set; }

        [JsonProperty("MilestoneMask")]
        public long MilestoneMask { get; set; }
    }

    public partial class Student
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("RecordBookID")]
        public long RecordBookId { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("SecondName")]
        public string SecondName { get; set; }

        public string ShortName() => $"{LastName} {FirstName}";

        [JsonProperty("GradeID")]
        public long GradeId { get; set; }

        [JsonProperty("GradeNum")]
        public long GradeNum { get; set; }

        [JsonProperty("Degree")]
        public Degree Degree { get; set; }

        [JsonProperty("GroupID")]
        public long GroupId { get; set; }

        [JsonProperty("GroupNum")]
        public long GroupNum { get; set; }

        [JsonProperty("IsAttached")]
        public bool IsAttached { get; set; }

        [JsonProperty("SubgroupID")]
        public long? SubgroupId { get; set; }
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
        public string ShortName() => $"{LastName} {FirstName[0]}. {SecondName[0]}.";
        public string FullName() => $"{LastName} {FirstName} {SecondName}";

        [JsonProperty("JobPositionID")]
        public long JobPositionId { get; set; }

        [JsonProperty("JobPositionName")]
        public string JobPositionName { get; set; }

        [JsonProperty("DepID")]
        public long? DepId { get; set; }

        [JsonProperty("DepName")]
        public string DepName { get; set; }

        [JsonProperty("FacultyID")]
        public long FacultyId { get; set; }

        [JsonProperty("FacultyAbbr")]
        public string FacultyAbbr { get; set; }

        [JsonProperty("IsAuthor")]
        public bool IsAuthor { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    public partial class Exam
    {
        [JsonProperty("Remove", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Remove { get; set; }

        [JsonProperty("Rate")]
        public long? Rate { get; set; }

        [JsonProperty("Absence", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Absence { get; set; }

        [JsonProperty("AutoPass", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AutoPass { get; set; }
    }

    public partial class Group
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("GroupNum")]
        public long GroupNum { get; set; }

        [JsonProperty("GradeID")]
        public long GradeId { get; set; }

        [JsonProperty("GradeNum")]
        public long GradeNum { get; set; }

        [JsonProperty("Degree")]
        public string Degree { get; set; }

        [JsonProperty("SpecID")]
        public long SpecId { get; set; }

        [JsonProperty("SpecName")]
        public string SpecName { get; set; }

        [JsonProperty("SpecAbbr")]
        public string SpecAbbr { get; set; }

        [JsonProperty("SpecCode")]
        public string SpecCode { get; set; }

        [JsonProperty("SpecYear")]
        public object SpecYear { get; set; }

        [JsonProperty("SpecFaculty")]
        public string SpecFaculty { get; set; }

        [JsonProperty("FormID")]
        public string FormId { get; set; }

        public string Name()
        {
            string res;
            if (Degree == "master")
                res = $"Магистратура, {GradeNum} год";
            else if (Degree == "specialist")
                res = $"Специалитет, {GradeNum} курс";
            else if (Degree == "postgraduate")
                res = $"Аспирантура, {GradeNum} год";
            else
                res = $"{GradeNum} курс";
            res += $" {GroupNum} группа";
            return res;
        }
    }

    public partial class Journal
    {
        [JsonProperty("LessonDate")]
        public DateTime LessonDate { get; set; }

        [JsonProperty("LessonType")]
        public string LessonType { get; set; }

        [JsonProperty("Attended")]
        public bool Attended { get; set; }

        public char AttendedSymbol { get => Attended == true ? '+' : '-'; }
    }

    public partial class LessonType
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("LessonType")]
        public string Type { get; set; }
    }

    public partial class Lesson
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("LessonDate")]
        public DateTime LessonDate { get; set; }

        [JsonProperty("LessonType")]
        public long LessonType { get; set; }

        [JsonProperty("LessonName")]
        public string LessonName { get; set; }

        [JsonProperty("SubgroupID")]
        public long? SubgroupId { get; set; }
    }

    public partial class Subgroup
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("TeacherID")]
        public long TeacherId { get; set; }

        [JsonProperty("DisciplineID")]
        public long DisciplineId { get; set; }
    }

    public partial class GymAttendanceInfo
    {
        [JsonProperty("DebtCount")]
        public int DebtCount { get; set; }

        [JsonProperty("SemesterCount")]
        public int SemesterCount { get; set; }

        [JsonProperty("Uncounted")]
        public int Uncounted { get; set; }

        [JsonProperty("TotalAttendance")]
        public int TotalAttendance { get; set; }
    }

    public partial class Milestone
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Mask")]
        public long Mask { get; set; }
    }


    public enum Degree { Bachelor, Master, Specialist, Postgraduate };

    public enum SubModuleType { CurrentControl, LandmarkControl };

    public enum ModuleType { Exam, Extra, Bonus, Regular };


    public partial struct TeacherValue
    {
        public TeacherElement[] TeacherElementArray;
        public Dictionary<string, TeacherElement> TeacherElementMap;

        public static implicit operator TeacherValue(TeacherElement[] TeacherElementArray) => new TeacherValue { TeacherElementArray = TeacherElementArray };
        public static implicit operator TeacherValue(Dictionary<string, TeacherElement> TeacherElementMap) => new TeacherValue { TeacherElementMap = TeacherElementMap };
    }

    public partial class StudentIndexResponse
    {
        public static StudentIndexResponse FromJson(string json) => JsonConvert.DeserializeObject<StudentIndexResponse>(json, Grade.Converter.Settings);
    }

    public partial class StudentDisciplineResponse
    {
        public static StudentDisciplineResponse FromJson(string json) => JsonConvert.DeserializeObject<StudentDisciplineResponse>(json, Grade.Converter.Settings);
    }

    public partial class SemesterListResponse
    {
        public static SemesterListResponse FromJson(string json) => JsonConvert.DeserializeObject<SemesterListResponse>(json, Grade.Converter.Settings);
    }
    public partial class TeacherIndexResponse
    {
        public static TeacherIndexResponse FromJson(string json) => JsonConvert.DeserializeObject<TeacherIndexResponse>(json, Grade.Converter.Settings);
    }

    public partial class TeacherDisciplineResponse
    {
        public static TeacherDisciplineResponse FromJson(string json) => JsonConvert.DeserializeObject<TeacherDisciplineResponse>(json, Grade.Converter.Settings);
    }
    public partial class StudentJournalResponse
    {
        public static StudentJournalResponse FromJson(string json) => JsonConvert.DeserializeObject<StudentJournalResponse>(json, Grade.Converter.Settings);
    }

    public partial class TeacherJournalResponse
    {
        public static TeacherJournalResponse FromJson(string json) => JsonConvert.DeserializeObject<TeacherJournalResponse>(json, Grade.Converter.Settings);
    }
    public partial class PostRequestResponse
    {
        public static PostRequestResponse FromJson(string json) => JsonConvert.DeserializeObject<PostRequestResponse>(json, Grade.Converter.Settings);
    }
    public partial class GetTokenResponse
    {
        public static GetTokenResponse FromJson(string json) => JsonConvert.DeserializeObject<GetTokenResponse>(json, Grade.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this StudentIndexResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this StudentDisciplineResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this SemesterListResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this TeacherIndexResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this TeacherDisciplineResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this StudentJournalResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this TeacherJournalResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this PostRequestResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
        public static string ToJson(this GetTokenResponse self) => JsonConvert.SerializeObject(self, Grade.Converter.Settings);
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
                DegreeConverter.Singleton,
                SubModuleTypeConverter.Singleton,
                ModuleTypeConverter.Singleton,
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
            if (long.TryParse(serializer.Deserialize<string>(reader), out long l))
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
    internal class DegreeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Degree) || t == typeof(Degree?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "bachelor" => Degree.Bachelor,
                "master" => Degree.Master,
                "specialist" => Degree.Specialist,
                "postgraduate" => Degree.Postgraduate,
                _ => throw new Exception("Cannot unmarshal type Degree"),
            };
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Degree)untypedValue;
            switch (value)
            {
                case Degree.Bachelor:
                    serializer.Serialize(writer, "bachelor");
                    return;
                case Degree.Master:
                    serializer.Serialize(writer, "master");
                    return;
                case Degree.Specialist:
                    serializer.Serialize(writer, "specialist");
                    return;
                case Degree.Postgraduate:
                    serializer.Serialize(writer, "postgraduate");
                    return;
            }
            throw new Exception("Cannot marshal type Degree");
        }

        public static readonly DegreeConverter Singleton = new DegreeConverter();
    }

    internal class SubModuleTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(SubModuleType) || t == typeof(SubModuleType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "CurrentControl" => SubModuleType.CurrentControl,
                "LandmarkControl" => SubModuleType.LandmarkControl,
                _ => throw new Exception("Cannot unmarshal type SubModuleType"),
            };
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (SubModuleType)untypedValue;
            switch (value)
            {
                case SubModuleType.CurrentControl:
                    serializer.Serialize(writer, "CurrentControl");
                    return;
                case SubModuleType.LandmarkControl:
                    serializer.Serialize(writer, "LandmarkControl");
                    return;
            }
            throw new Exception("Cannot marshal type SubModuleType");
        }

        public static readonly SubModuleTypeConverter Singleton = new SubModuleTypeConverter();
    }

    internal class ModuleTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ModuleType) || t == typeof(ModuleType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "exam" => ModuleType.Exam,
                "extra" => ModuleType.Extra,
                "bonus" => ModuleType.Bonus,
                "regular" => ModuleType.Regular,
                _ => throw new Exception("Cannot unmarshal type ModuleType"),
            };
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (ModuleType)untypedValue;
            switch (value)
            {
                case ModuleType.Exam:
                    serializer.Serialize(writer, "exam");
                    return;
                case ModuleType.Extra:
                    serializer.Serialize(writer, "extra");
                    return;
                case ModuleType.Bonus:
                    serializer.Serialize(writer, "bonus");
                    return;
                case ModuleType.Regular:
                    serializer.Serialize(writer, "regular");
                    return;
            }
            throw new Exception("Cannot marshal type ModuleType");
        }

        public static readonly ModuleTypeConverter Singleton = new ModuleTypeConverter();
    }
}
