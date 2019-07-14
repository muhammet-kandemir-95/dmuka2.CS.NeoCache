using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dmuka2.CS.NeoCache.TestApp.Admin.Models.Home;

namespace dmuka2.CS.NeoCache.TestApp.Admin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetJson([FromBody] GetJsonModel model)
        {
            model.ClassId = "class_" + model.ClassId;
            List<object> result = new List<object>();
            using (BrainClient client = new BrainClient())
            {
                client.Open(model.Host, model.Port, model.Password);

                try
                {
                    string classLabel = "Class-" + client.GetNeuronValueAsString(model.ClassId, "name");
                    string teacherLabel = "Teacher-" + client.GetNeuronValueAsString(model.ClassId, "teacher.name") + " " + client.GetNeuronValueAsString(model.ClassId, "teacher.surname") + " ( " + client.GetNeuronValueAsInt16(model.ClassId, "teacher.birth_year") + " )";
                    result.Add(new
                    {
                        source = classLabel,
                        target = teacherLabel
                    });

                    var studentsLength = client.GetNeuronValueAsInt32(model.ClassId, "students.{len}");
                    for (int i = 0; i < studentsLength; i++)
                    {
                        string studentLabel = "Student-" + client.GetNeuronValueAsString(model.ClassId, "students[" + i + "].name") + " " + client.GetNeuronValueAsString(model.ClassId, "students[" + i + "].surname") + " ( " + client.GetNeuronValueAsInt16(model.ClassId, "students[" + i + "].birth_year") + " )";
                        result.Add(new
                        {
                            source = classLabel,
                            target = studentLabel
                        });

                        var examsLength = client.GetNeuronValueAsInt32(model.ClassId, "students[" + i + "].exams.{len}");
                        for (int o = 0; o < examsLength; o++)
                        {
                            string examLabel = "Exam-" + client.GetNeuronValueAsString(model.ClassId, "students[" + i + "].exams[" + o + "].name") + " ( " + client.GetNeuronValueAsByte(model.ClassId, "students[" + i + "].exams[" + o + "].result") + " )";
                            result.Add(new
                            {
                                source = studentLabel,
                                target = examLabel
                            });
                        }
                    }
                }
                catch
                {
                    client.Close();
                    throw;
                }

                client.Close();
            }

            return Json(result);
        }
    }
}
