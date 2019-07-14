using System;
using System.Threading;

namespace dmuka2.CS.NeoCache.TestApp.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===================== Welcome Brain Server =====================");
            Console.WriteLine("                      (Muhammet KANDEMİR)");

            Console.Write("Please write a password = ");
            string pass = Console.ReadLine();

            Console.Write("Please write a port = ");
            int port = Convert.ToInt32(Console.ReadLine());

            BrainServer server = null;
            new Thread(() =>
            {
                Model.Class test = new Model.Class();
                server = new BrainServer(4, pass, port);
                server.Open();
            }).Start();

            while (server == null || server.Enable == false)
                Thread.Sleep(1);

            Console.WriteLine("INFO : Server started.");
            Console.WriteLine("WARN : Server filling default datas...");

            BrainClient client = new BrainClient();
            client.Open("localhost", port, pass);

            // Class (A-1)
            client.AddANeuron("class_A-1", "Class");
            client.SetNeuronValue("class_A-1", $"name", "A-1");
            // Class (A-1) - Teacher (Medine KANDEMIR)
            client.SetNeuronValue("class_A-1", "teacher.name", "Medine");
            client.SetNeuronValue("class_A-1", "teacher.surname", "KANDEMIR");
            client.SetNeuronValue("class_A-1", "teacher.birth_year", (short)1989);
            // Class (A-1) - Student[0] (Muhammet KANDEMIR)
            client.AddANeuronToList("class_A-1", "students");
            client.SetNeuronValue("class_A-1", "students[0].name", "Muhammet");
            client.SetNeuronValue("class_A-1", "students[0].surname", "KANDEMIR");
            client.SetNeuronValue("class_A-1", "students[0].birth_year", (short)1995);
            // Class (A-1) - Student[0] (Muhammet KANDEMIR) - Exam[0] (Math 100)
            client.AddANeuronToList("class_A-1", "students[0].exams");
            client.SetNeuronValue("class_A-1", "students[0].exams[0].name", "Math");
            client.SetNeuronValue("class_A-1", "students[0].exams[0].result", (byte)100);
            // Class (A-1) - Student[0] (Muhammet KANDEMIR) - Exam[1] (Music 100)
            client.AddANeuronToList("class_A-1", "students[0].exams");
            client.SetNeuronValue("class_A-1", "students[0].exams[1].name", "Music");
            client.SetNeuronValue("class_A-1", "students[0].exams[1].result", (byte)100);
            // Class (A-1) - Student[0] (Muhammet KANDEMIR) - Exam[2] (Sport 100)
            client.AddANeuronToList("class_A-1", "students[0].exams");
            client.SetNeuronValue("class_A-1", "students[0].exams[2].name", "Sport");
            client.SetNeuronValue("class_A-1", "students[0].exams[2].result", (byte)100);
            // Class (A-1) - Student[1] (Nesibe KANDEMIR)
            client.AddANeuronToList("class_A-1", "students");
            client.SetNeuronValue("class_A-1", "students[1].name", "Nesibe");
            client.SetNeuronValue("class_A-1", "students[1].surname", "KANDEMIR");
            client.SetNeuronValue("class_A-1", "students[1].birth_year", (short)1996);
            // Class (A-1) - Student[1] (Nesibe KANDEMIR) - Exam[0] (Math 72)
            client.AddANeuronToList("class_A-1", "students[1].exams");
            client.SetNeuronValue("class_A-1", "students[1].exams[0].name", "Math");
            client.SetNeuronValue("class_A-1", "students[1].exams[0].result", (byte)72);
            // Class (A-1) - Student[1] (Nesibe KANDEMIR) - Exam[1] (Music 55)
            client.AddANeuronToList("class_A-1", "students[1].exams");
            client.SetNeuronValue("class_A-1", "students[1].exams[1].name", "Music");
            client.SetNeuronValue("class_A-1", "students[1].exams[1].result", (byte)55);
            // Class (A-1) - Student[1] (Nesibe KANDEMIR) - Exam[2] (Sport 29)
            client.AddANeuronToList("class_A-1", "students[1].exams");
            client.SetNeuronValue("class_A-1", "students[1].exams[2].name", "Sport");
            client.SetNeuronValue("class_A-1", "students[1].exams[2].result", (byte)29);
            // Class (A-1) - Student[2] (Omer KANDEMIR)
            client.AddANeuronToList("class_A-1", "students");
            client.SetNeuronValue("class_A-1", "students[2].name", "Omer");
            client.SetNeuronValue("class_A-1", "students[2].surname", "KANDEMIR");
            client.SetNeuronValue("class_A-1", "students[2].birth_year", (short)1995);
            // Class (A-1) - Student[2] (Omer KANDEMIR) - Exam[0] (Math 51)
            client.AddANeuronToList("class_A-1", "students[2].exams");
            client.SetNeuronValue("class_A-1", "students[2].exams[0].name", "Math");
            client.SetNeuronValue("class_A-1", "students[2].exams[0].result", (byte)51);
            // Class (A-1) - Student[2] (Omer KANDEMIR) - Exam[1] (Music 74)
            client.AddANeuronToList("class_A-1", "students[2].exams");
            client.SetNeuronValue("class_A-1", "students[2].exams[1].name", "Music");
            client.SetNeuronValue("class_A-1", "students[2].exams[1].result", (byte)74);
            // Class (A-1) - Student[2] (Omer KANDEMIR) - Exam[2] (Sport 64)
            client.AddANeuronToList("class_A-1", "students[2].exams");
            client.SetNeuronValue("class_A-1", "students[2].exams[2].name", "Sport");
            client.SetNeuronValue("class_A-1", "students[2].exams[2].result", (byte)64);

            // Class (A-2)
            client.AddANeuron("class_A-2", "Class");
            client.SetNeuronValue("class_A-2", $"name", "A-2");
            // Class (A-2) - Teacher (Remzi KANDEMIR)
            client.SetNeuronValue("class_A-2", "teacher.name", "Remzi");
            client.SetNeuronValue("class_A-2", "teacher.surname", "KANDEMIR");
            client.SetNeuronValue("class_A-2", "teacher.birth_year", (short)1990);
            // Class (A-2) - Student[0] (Tulin KANDEMIR)
            client.AddANeuronToList("class_A-2", "students");
            client.SetNeuronValue("class_A-2", "students[0].name", "Tulin");
            client.SetNeuronValue("class_A-2", "students[0].surname", "KANDEMIR");
            client.SetNeuronValue("class_A-2", "students[0].birth_year", (short)2000);
            // Class (A-2) - Student[0] (Tulin KANDEMIR) - Exam[0] (Music 43)
            client.AddANeuronToList("class_A-2", "students[0].exams");
            client.SetNeuronValue("class_A-2", "students[0].exams[0].name", "Music");
            client.SetNeuronValue("class_A-2", "students[0].exams[0].result", (byte)43);
            // Class (A-2) - Student[0] (Tulin KANDEMIR) - Exam[1] (Sport 98)
            client.AddANeuronToList("class_A-2", "students[0].exams");
            client.SetNeuronValue("class_A-2", "students[0].exams[1].name", "Sport");
            client.SetNeuronValue("class_A-2", "students[0].exams[1].result", (byte)98);
            // Class (A-2) - Student[1] (Hamza KANDEMIR)
            client.AddANeuronToList("class_A-2", "students");
            client.SetNeuronValue("class_A-2", "students[1].name", "Hamza");
            client.SetNeuronValue("class_A-2", "students[1].surname", "KANDEMIR");
            client.SetNeuronValue("class_A-2", "students[1].birth_year", (short)2002);
            // Class (A-2) - Student[1] (Hamza KANDEMIR) - Exam[0] (Music 100)
            client.AddANeuronToList("class_A-2", "students[1].exams");
            client.SetNeuronValue("class_A-2", "students[1].exams[0].name", "Music");
            client.SetNeuronValue("class_A-2", "students[1].exams[0].result", (byte)100);
            // Class (A-2) - Student[1] (Hamza KANDEMIR) - Exam[1] (Sport 24)
            client.AddANeuronToList("class_A-2", "students[1].exams");
            client.SetNeuronValue("class_A-2", "students[1].exams[1].name", "Sport");
            client.SetNeuronValue("class_A-2", "students[1].exams[1].result", (byte)24);
            // Class (A-2) - Student[2] (Mehmet KANDEMIR)
            client.AddANeuronToList("class_A-2", "students");
            client.SetNeuronValue("class_A-2", "students[2].name", "Mehmet");
            client.SetNeuronValue("class_A-2", "students[2].surname", "KANDEMIR");
            client.SetNeuronValue("class_A-2", "students[2].birth_year", (short)2003);
            // Class (A-2) - Student[2] (Mehmet KANDEMIR) - Exam[0] (Music 57)
            client.AddANeuronToList("class_A-2", "students[2].exams");
            client.SetNeuronValue("class_A-2", "students[2].exams[0].name", "Music");
            client.SetNeuronValue("class_A-2", "students[2].exams[0].result", (byte)57);
            // Class (A-2) - Student[2] (Mehmet KANDEMIR) - Exam[1] (Sport 19)
            client.AddANeuronToList("class_A-2", "students[2].exams");
            client.SetNeuronValue("class_A-2", "students[2].exams[1].name", "Sport");
            client.SetNeuronValue("class_A-2", "students[2].exams[1].result", (byte)19);
            // Class (A-2) - Student[3] (Ali KANDEMIR)
            client.AddANeuronToList("class_A-2", "students");
            client.SetNeuronValue("class_A-2", "students[3].name", "Ali");
            client.SetNeuronValue("class_A-2", "students[3].surname", "KANDEMIR");
            client.SetNeuronValue("class_A-2", "students[3].birth_year", (short)2003);
            // Class (A-2) - Student[3] (Ali KANDEMIR) - Exam[0] (Music 84)
            client.AddANeuronToList("class_A-2", "students[3].exams");
            client.SetNeuronValue("class_A-2", "students[3].exams[0].name", "Music");
            client.SetNeuronValue("class_A-2", "students[3].exams[0].result", (byte)84);
            // Class (A-2) - Student[3] (Ali KANDEMIR) - Exam[1] (Sport 89)
            client.AddANeuronToList("class_A-2", "students[3].exams");
            client.SetNeuronValue("class_A-2", "students[3].exams[1].name", "Sport");
            client.SetNeuronValue("class_A-2", "students[3].exams[1].result", (byte)89);

            client.Close();
            Console.WriteLine("INFO : Server filled with default datas.");

            while (true)
                Thread.Sleep(1000);
        }
    }
}
