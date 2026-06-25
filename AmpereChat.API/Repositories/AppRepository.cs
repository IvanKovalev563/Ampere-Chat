using AmpereChat.API.Models;
using Dapper;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace AmpereChat.API.Repositories
{
    public class AppRepository
    {
        private readonly string _connectionString;
        public AppRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // ---- Users ----
        public async Task<User?> GetUserByUsername(string username)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = "SELECT user_id, username, password_hash FROM User WHERE username = @username";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { username });
        }

        public async Task<int> CreateUser(string username, string passwordHash)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = "INSERT INTO User (username, password_hash) VALUES (@username, @passwordHash); SELECT LAST_INSERT_ID();";
            return await conn.ExecuteScalarAsync<int>(sql, new { username, passwordHash });
        }
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = "SELECT user_id, username FROM User";
            return await conn.QueryAsync<User>(sql);
        }

        // ---- Messages ----
        public async Task<IEnumerable<Message>> GetMessages(int chatId, int limit = 50)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = @"SELECT message_id, chat_id, user_id, text, sent_at 
                        FROM Message 
                        WHERE chat_id = @chatId 
                        ORDER BY sent_at DESC 
                        LIMIT @limit";
            return await conn.QueryAsync<Message>(sql, new { chatId, limit });
        }

        public async Task<int> AddMessage(int chatId, int userId, string text)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = @"INSERT INTO Message (chat_id, user_id, text, sent_at) 
                        VALUES (@chatId, @userId, @text, NOW());
                        SELECT LAST_INSERT_ID();";
            return await conn.ExecuteScalarAsync<int>(sql, new { chatId, userId, text });
        }

        // ---- Chats ----
        public async Task<Chat?> GetChatById(int chatId)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = "SELECT chat_id, name, is_group FROM Chat WHERE chat_id = @chatId";
            return await conn.QueryFirstOrDefaultAsync<Chat>(sql, new { chatId });
        }
        public async Task<IEnumerable<dynamic>> GetChatsForUser(int userId)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = @"
                        SELECT 
                            c.chat_id,
                            CASE 
                                WHEN c.is_group = 1 THEN c.name
                                ELSE (SELECT u.username 
                                      FROM Chat_Participant cp2 
                                      JOIN User u ON cp2.user_id = u.user_id 
                                      WHERE cp2.chat_id = c.chat_id AND cp2.user_id != @userId)
                            END AS display_name,
                            c.is_group,
                            c.created_at,
                            (SELECT text FROM Message m WHERE m.chat_id = c.chat_id ORDER BY m.sent_at DESC LIMIT 1) AS last_message,
                            (SELECT sent_at FROM Message m WHERE m.chat_id = c.chat_id ORDER BY m.sent_at DESC LIMIT 1) AS last_message_time
                        FROM Chat_Participant cp
                        JOIN Chat c ON cp.chat_id = c.chat_id
                        WHERE cp.user_id = @userId";
            return await conn.QueryAsync(sql, new { userId });
        }
        public async Task<int?> GetPrivateChatBetweenUsers(int userId1, int userId2)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = @"
                        SELECT cp.chat_id
                        FROM Chat_Participant cp
                        JOIN Chat c ON cp.chat_id = c.chat_id
                        WHERE cp.user_id IN (@userId1, @userId2) AND c.is_group = 0
                        GROUP BY cp.chat_id
                        HAVING COUNT(DISTINCT cp.user_id) = 2 AND COUNT(*) = 2";
            return await conn.QueryFirstOrDefaultAsync<int?>(sql, new { userId1, userId2 });
        }

        public async Task<int> CreatePrivateChat(int userId1, int userId2)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var trans = await conn.BeginTransactionAsync();

            try
            {
                var insertChatSql = "INSERT INTO Chat (is_group) VALUES (0); SELECT LAST_INSERT_ID();";
                var chatId = await conn.ExecuteScalarAsync<int>(insertChatSql, transaction: trans);

                var insertParticipantSql = "INSERT INTO Chat_Participant (chat_id, user_id) VALUES (@chatId, @userId)";
                await conn.ExecuteAsync(insertParticipantSql, new { chatId, userId = userId1 }, transaction: trans);
                await conn.ExecuteAsync(insertParticipantSql, new { chatId, userId = userId2 }, transaction: trans);

                await trans.CommitAsync();
                return chatId;
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }
        public async Task<MessageWithUser?> GetMessageByIdWithUser(int messageId)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = @"
        SELECT m.message_id, m.chat_id, m.user_id, m.text, m.sent_at, u.username
        FROM Message m
        JOIN User u ON m.user_id = u.user_id
        WHERE m.message_id = @messageId";
            return await conn.QueryFirstOrDefaultAsync<MessageWithUser>(sql, new { messageId });
        }
    }
}
