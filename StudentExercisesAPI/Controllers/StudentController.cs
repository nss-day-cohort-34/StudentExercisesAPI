using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private IConfiguration _config;

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, 
                                   s.CohortId, c.Name AS CohortName,
                                   se.ExerciseId, e.Name AS ExerciseName, e.Language
                              FROM Student s INNER JOIN Cohort c ON s.CohortId = c.id
                                   LEFT JOIN StudentExercise se on se.StudentId = s.id
                                   LEFT JOIN Exercise e on se.ExerciseId = e.Id";

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Student> students = new Dictionary<int, Student>();

                    while (reader.Read())
                    {
                        int studentId = reader.GetInt32(reader.GetOrdinal("Id"));
                        if (! students.ContainsKey(studentId))
                        {
                            Student newStudent = new Student()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                }
                            };

                            students.Add(studentId, newStudent);
                        }

                        Student fromDictionary = students[studentId];

                        if (! reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                        {
                            Exercise anExercise = new Exercise()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                Language = reader.GetString(reader.GetOrdinal("Language"))
                            };
                            fromDictionary.Exercises.Add(anExercise);
                        }
                    }

                    reader.Close();

                    return Ok(students.Values);
                }
            }
        }
    }
}