using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiBasicAuth.Models;
using DataAcces.Context;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net.Http;

namespace ApiBasicAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Posts>>> GetPost()
        {
            return await _context.Posts.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Posts>> GetPost(int id)
        {
            try
            {
                List<Posts> posts = new List<Posts>();
                using (var connection = (SqlConnection)_context.Database.GetDbConnection())
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        await connection.OpenAsync();
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "GetPostById";
                        cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            Posts post = null;
                            while (await reader.ReadAsync())
                            {
                                post = new Posts();
                                post.UserId = (int)reader["UserId"];
                                post.Id = (int)reader["Id"];
                                post.Title = (string)reader["Title"];
                                post.Body = (string)reader["Body"];
                                posts.Add(post);
                            }
                        }
                    }
                }
                if (posts == null)
                {
                    return NotFound();
                }
                return posts[0];
            }
            catch (Exception)
            {
                throw;
            }
        }



        // PUT: api/Posts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Posts post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            var connection = (SqlConnection)_context.Database.GetDbConnection();
            try
            {
                SqlCommand cmd = connection.CreateCommand();
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "UpdatePost";
                cmd.Parameters.AddWithValue("@UserId", post.UserId);
                cmd.Parameters.AddWithValue("@Id", post.Id);
                cmd.Parameters.AddWithValue("@Title", post.Title);
                cmd.Parameters.AddWithValue("@Body", post.Body);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }

            return NoContent();
        }


        // POST: api/Posts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Posts>> PostPost(Posts post)
        {
            try
            {
                var connection = (SqlConnection)_context.Database.GetDbConnection();
                SqlCommand cmd = connection.CreateCommand();
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "InsertPost";
                cmd.Parameters.Add("@UserId", System.Data.SqlDbType.Int).Value = post.UserId;
                cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = post.Id;
                cmd.Parameters.Add("@Title", System.Data.SqlDbType.VarChar).Value = post.Title;
                cmd.Parameters.Add("@Body", System.Data.SqlDbType.Text).Value = post.Body;
                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return CreatedAtAction("GetPost", new { id = post.Id }, post);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var connection = (SqlConnection)_context.Database.GetDbConnection();
                SqlCommand cmd = connection.CreateCommand();
                connection.Open();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "DeletePost";
                cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
                await cmd.ExecuteNonQueryAsync();
                connection.Close();

                return NoContent();
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost("populate")]
        public async Task<IActionResult> PopulateData()
        {
            using var client = new HttpClient();

            // Obtener los posts de jsonplaceholder
            var postsResponse = await client.GetAsync("https://jsonplaceholder.typicode.com/posts");
            var postsContent = await postsResponse.Content.ReadAsStringAsync();
            var posts = JsonConvert.DeserializeObject<List<Posts>>(postsContent);

            
            // Guardar los posts en la base de datos
            await _context.Posts.AddRangeAsync(posts);
            await _context.SaveChangesAsync();


            return Ok();
        }


    }
}
