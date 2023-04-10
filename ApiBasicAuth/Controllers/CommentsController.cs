using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiBasicAuth.Models;
using DataAcces.Context;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net.Http;

namespace ApiBasicAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Comments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comments>>> GetComment()
        {
            return await _context.Comments.ToListAsync();
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Comments>>> GetComment(int id)
        {
            try
            {
                var connection = (SqlConnection)_context.Database.GetDbConnection();
                SqlCommand cmd = connection.CreateCommand();
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "GetCommentsByPostId";
                cmd.Parameters.Add("@PostId", System.Data.SqlDbType.Int).Value = id;
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                List<Comments> comments = new List<Comments>();

                while (await reader.ReadAsync())
                {
                    Comments comment = new Comments();
                    comment.PostId = (int)reader["PostId"];
                    comment.Id = (int)reader["Id"];
                    comment.Name = (string)reader["Name"];
                    comment.Email = (string)reader["Email"];
                    comment.Body = (string)reader["Body"];
                    comments.Add(comment);
                }

                connection.Close();

                if (comments.Count == 0)
                {
                    return NotFound();
                }

                return comments;
            }
            catch (Exception)
            {
                throw;
            }
        }


        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, Comments comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            try
            {
                var connection = (SqlConnection)_context.Database.GetDbConnection();
                SqlCommand cmd = connection.CreateCommand();
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "UpdateComment";
                cmd.Parameters.Add("@PostId", System.Data.SqlDbType.Int).Value = comment.PostId;
                cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = comment.Id;
                cmd.Parameters.Add("@Name", System.Data.SqlDbType.VarChar).Value = comment.Name;
                cmd.Parameters.Add("@Email", System.Data.SqlDbType.VarChar).Value = comment.Email;
                cmd.Parameters.Add("@Body", System.Data.SqlDbType.Text).Value = comment.Body;
                await cmd.ExecuteNonQueryAsync();
                connection.Close();
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }


        // POST: api/Comments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Comments>> PostComment(Comments comment)
        {
            try
            {
                var connection = (SqlConnection)_context.Database.GetDbConnection();
                SqlCommand cmd = connection.CreateCommand();
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "InsertComment";
                cmd.Parameters.Add("@PostId", System.Data.SqlDbType.Int).Value = comment.PostId;
                cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = comment.Id;
                cmd.Parameters.Add("@Name", System.Data.SqlDbType.VarChar).Value = comment.Name;
                cmd.Parameters.Add("@Email", System.Data.SqlDbType.VarChar).Value = comment.Email;
                cmd.Parameters.Add("@Body", System.Data.SqlDbType.Text).Value = comment.Body;
                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return CreatedAtAction("GetComment", new { id = comment.Id }, comment);
            }
            catch (Exception)
            {
                throw;
            }
        }


        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var connection = (SqlConnection)_context.Database.GetDbConnection();
                SqlCommand cmd = connection.CreateCommand();
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "DeleteComment";
                cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
                await cmd.ExecuteNonQueryAsync();
                connection.Close();
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }
        [HttpPost("populate")]
        public async Task<IActionResult> PopulateData()
        {
            using var client = new HttpClient();

       
            // Obtener los comentarios de jsonplaceholder
            var commentsResponse = await client.GetAsync("https://jsonplaceholder.typicode.com/comments");
            var commentsContent = await commentsResponse.Content.ReadAsStringAsync();
            var comments = JsonConvert.DeserializeObject<List<Comments>>(commentsContent);
           
            // Guardar los comentarios en la base de datos
            await _context.Comments.AddRangeAsync(comments);
            await _context.SaveChangesAsync();

            return Ok();
        }



    }
}
