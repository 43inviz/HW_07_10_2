using Microsoft.EntityFrameworkCore;

namespace HW_07_10_2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await InitializeAsync();
            try
            {
                User user = await LoadUserDataAsync();
                if (user != null)
                {
                    Console.WriteLine($"{user.Id}, Name: {user.Name}, email: {user.Email}");
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Запрос был отменен из за ожидания");
            }
        }


        static async Task<User?> LoadUserDataAsync()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            CancellationToken cancellationToken= cancellationTokenSource.Token;


            Task<User> loadTask = Task.Run(async () => {
                await Task.Delay(TimeSpan.FromSeconds(11));
                using (ApplicationContext db = new ApplicationContext())
                {
                    return db.Users.FirstOrDefault()!;
                    //User? user = await db.Users.FirstOrDefaultAsync(cancellationToken);
                    //return user;
                }

            });

            if(await Task.WhenAny(loadTask,Task.Delay(TimeSpan.FromSeconds(10),cancellationToken)) == loadTask)
            {
                return await loadTask;
            }
            else
            {
                throw new OperationCanceledException("Запрос к серверу был отменен за-за длительного ожидания");
            }
        }


        static async Task InitializeAsync()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureCreated();
                db.Database.EnsureDeleted();


                db.Users.AddRange(
                        new User[]
                        {
                            new User{Name = "Tom" , Email = "user1@gmail.com"},
                            new User{Name = "Dom", Email = "user2@gmail.com"}
                        }
                );

                db.SaveChanges();
            }
        }
    }


    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

    }

    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-R3LQDV9;Database = TestDb1;Trusted_Connection =True;TrustServerCertificate=True");
        }
    }

}
