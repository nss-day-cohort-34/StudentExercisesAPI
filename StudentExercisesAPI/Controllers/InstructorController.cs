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
    public class InstructorController : ControllerBase
    {
        private IConfiguration _config;

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public InstructorController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Get(string firstName, string sort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, FirstName, LastName, SlackHandle, CohortId
                                          FROM Instructor
                                         WHERE FirstName LIKE @firstName";
                    if (sort?.ToLower() == "firstname")
                    {
                        cmd.CommandText += " ORDER BY FirstName";
                    }
                    else if (sort == "LastName")
                    {
                        cmd.CommandText += " ORDER BY LastName";
                    }

                    cmd.Parameters.Add(new SqlParameter("@firstName", $"%{firstName}%"));
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Instructor> instructors = new List<Instructor>();

                    while (reader.Read())
                    {
                        instructors.Add(
                            new Instructor()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            });
                    }

                    reader.Close();

                    return Ok(instructors);
                }
            }
        }

        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.FirstName, i.LastName, 
                                               i.SlackHandle, i.CohortId, 
                                               c.Name AS CohortName
                                          FROM Instructor i INNER JOIN cohort c on c.id = i.cohortId
                                         WHERE i.id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Instructor instructor = null;
                    if (reader.Read())
                    {
                        instructor = new Instructor()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                }
                            };
                    }

                    reader.Close();

                    return Ok(instructor);
                }
            }
        }

        [HttpPost]
        public IActionResult Post(Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructor (FirstName, LastName, SlackHandle, CohortId)
                                            VALUES (@firstname, @lastname, @slackhandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", instructor.CohortId));

                    cmd.ExecuteNonQuery();

                    return Ok();
                }
            }
        }
     }
}