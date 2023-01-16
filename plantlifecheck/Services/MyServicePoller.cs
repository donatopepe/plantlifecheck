using Areas.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using plantlifecheck.Data;
using plantlifecheck.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace plantlifecheck.Services
{
    public class MyServicePoller : BackgroundService
    {
        private ApplicationDbContext _context;
        private UserManager<CustomUser> userManager;
        private RoleManager<CustomRole> roleManager;
        private readonly ILogger<MyServicePoller> _logger;
        private readonly IConfiguration Configuration;
        private readonly IEmailSender _emailSender;
        private readonly MailSmsSender _sendSmsMail;
        private readonly SqlConfigs _sqlConfigs;
        private readonly ILoggerFactory _loggerFactory;

        public static bool CheckConnection(ApplicationDbContext context)
        {
            if (context == null) return false;
            try
            {
                return context.Database.CanConnect();
            }
            catch (SqlException)
            {
                return false;
            }
        }

        private ApplicationDbContext ConnectDatabase()
        {
            if (_context == null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("ApplicationDbContext"));
                //_logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>MyServiceReader connection:" + Configuration.GetConnectionString("DefaultConnection"));
                _context = new ApplicationDbContext(optionsBuilder.Options, new Logger<ApplicationDbContext>(_loggerFactory));
            }

            if (!CheckConnection(_context))
            {
                _logger.LogError(">>>>>>>>>> Database non accessibile");
                _context = null;
                return null;
            }
            return _context;
        }

        public MyServicePoller(IConfiguration configuration, IOptions<SqlConfigs> sqlConfigs, IEmailSender emailSender, ILoggerFactory loggerFactory, MailSmsSender sendSmsMail)
        {
            Configuration = configuration;
            _emailSender = emailSender;
            _logger = new Logger<MyServicePoller>(loggerFactory); ;
            _sendSmsMail = sendSmsMail;
            _sqlConfigs = sqlConfigs.Value;

            _loggerFactory = loggerFactory;
        }

        private void provaquery()
        {
            string jsonString = JsonSerializer.Serialize(_sqlConfigs);
            _logger.LogInformation(jsonString);

            //var connectionStrings = new Dictionary<string, string>();
            //connectionStrings.Add("DWH", "Server=[REDACTED];Database=[REDACTED];User Id=[REDACTED];$1=[REDACTED];");
            //var connections = new Connection(connectionStrings);
            //var parameters = new Dictionary<string, object>();
            //parameters.Add("@pSearchDate", DateTime.Today);
            //string sql = "spCoilCodeWeightGetSum";
            //var storeprocedure = QueryCommandExecutor.ExecuteQueryDictionarySp(connections.Get("DWH"), sql, parameters);

            //foreach (var entry in storeprocedure)
            //    foreach (var colonne in entry)
            //        Console.WriteLine("[{0} {1}]", colonne.Key, colonne.Value == null ? "null" : colonne.Value);

            var queryConsuntivi = new QueryConsuntivi();
            foreach (var connection in _sqlConfigs.Configs)
            {

                foreach (var query in connection.Queries)
                {

                    var returns = new Dictionary<string, object>();
                    queryConsuntivi.Consuntivi.Add(query.Name, new QueryConsuntivo());
                    var parameters = new Dictionary<string, object>();
                    parameters.Add(query.DataInizio, DateTime.Today.AddDays(-1).Date);
                    parameters.Add(query.DataFine, DateTime.Today.AddDays(+1).Date);
                    foreach (var parameter in query.ParametersList)
                    {
                        if (parameter.Value != null)
                            parameters.Add(parameter.Parameter, parameter.Value);
                    }
                    if (query.CommandType == CommandType.StoredProcedure)
                    {
                        //var storeprocedure = QueryCommandExecutor.ExecuteQueryDictionarySp(connectionString, query.Sql, parameters);
                        var storeprocedure = QueryCommandExecutor.ExecuteQueryDictionarySp(new SqlConnection(connection.ConnectionString), query.Sql, parameters);
                        foreach (var ritorno in query.Returns)
                        {
                            //Console.WriteLine(ritorno.DataInizio + ":" + storeprocedure[0][ritorno.DataInizio]);
                            //Console.WriteLine(ritorno.DataFine + ":" + storeprocedure[0][ritorno.DataFine]);
                            //Console.WriteLine(ritorno.Consuntivo + ":" + storeprocedure[0][ritorno.Consuntivo]);
                            queryConsuntivi.Consuntivi[query.Name].DataInizio = Convert.ToDateTime(storeprocedure[0][ritorno.DataInizio]);
                            queryConsuntivi.Consuntivi[query.Name].DataFine = Convert.ToDateTime(storeprocedure[0][ritorno.DataFine]);
                            queryConsuntivi.Consuntivi[query.Name].Consuntivo = Convert.ToInt64(storeprocedure[0][ritorno.Consuntivo]);
                        }

                        //foreach (var entry in storeprocedure)
                        //{
                        //    foreach (var ritorno in query.Returns)
                        //    {
                        //        Console.WriteLine(ritorno.DataInizio+":"+entry[ritorno.DataInizio]);
                        //        Console.WriteLine(ritorno.DataFine + ":" + entry[ritorno.DataFine]);
                        //        Console.WriteLine(ritorno.Consuntivo + ":" + entry[ritorno.Consuntivo]);
                        //    }
                        //    //foreach (var colonne in entry)
                        //    //{
                        //    //    returns.Add(colonne.Key, colonne.Value);
                        //    //    Console.WriteLine("[{0} {1}]", colonne.Key, colonne.Value == null ? "null" : colonne.Value);
                        //    //}
                        //}
                    }


                }
            }
            foreach (var qconsuntivo in queryConsuntivi.Consuntivi)
            {
                Console.WriteLine("Query" + ":" + qconsuntivo.Key);
                Console.WriteLine("DataInizio" + ":" + qconsuntivo.Value.DataInizio);
                Console.WriteLine("DataFine" + ":" + qconsuntivo.Value.DataFine);
                Console.WriteLine("Consuntivo" + ":" + qconsuntivo.Value.Consuntivo);

            }


        }




        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Generate a random number  
            Random random = new Random();
            int randomLessThan10 = random.Next(10);
            //await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            _logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>MyServiceReader is starting.");

            //_logger.LogInformation(AllarmiSMS.ComponiInviaSMS("Prova invio sms","393283387195"));

            stoppingToken.Register(() => _logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>MyServiceReader is stopping."));


            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {

                    var controlliVita = await ConnectDatabase().ControlloVita.Include(s => s.Utente).Include(r => r.Componente).Include(m => m.Impianto).ThenInclude(n => n.Nome).Include(m => m.Macchina).ThenInclude(n => n.Nome).Where(s => s.Abilitato).ToListAsync(cancellationToken: stoppingToken);
                    if (controlliVita != null)
                    {
                        var queryConsuntivi = new QueryConsuntivi();
                        var controlliVitaQuery = controlliVita.Where(s => (s.SP_Consuntivo_Abilitato)&&(s.SP_Consuntivo!=null));
                        var controlliVitaNoQuery = controlliVita.Where(s => !(s.SP_Consuntivo_Abilitato));
                        if (controlliVitaQuery != null)
                        {
                            foreach (var connection in _sqlConfigs.Configs)
                            {
                                foreach (var query in connection.Queries)
                                {
                                    var controlloVitaQuery = controlliVita.Where(s => s.SP_Consuntivo == query.Name);
                                    if (controlloVitaQuery != null)
                                    {
                                        foreach (var controllovita in controlloVitaQuery)
                                        {
                                            var parameters = new Dictionary<string, object>();
                                            parameters.Add(query.DataInizio, controllovita.Partenza);
                                            parameters.Add(query.DataFine, DateTime.Today.AddDays(0).Date);
                                            if (query.ParametersList != null)
                                                foreach (var parameter in query.ParametersList)
                                                {
                                                    if (parameter.Value != null)
                                                        parameters.Add(parameter.Parameter, parameter.Value);
                                                }
                                            if (query.CommandType == CommandType.StoredProcedure)
                                            {
                                                //var storeprocedure = QueryCommandExecutor.ExecuteQueryDictionarySp(connectionString, query.Sql, parameters);
                                                var storeprocedure = QueryCommandExecutor.ExecuteQueryDictionarySp(new SqlConnection(connection.ConnectionString), query.Sql, parameters);
                                                var ritorno = query.Returns.FirstOrDefault();
                                                controllovita.Ultimo_aggiornamento_Consuntivo = Convert.ToDateTime(storeprocedure[0][ritorno.DataFine]);
                                                controllovita.Consuntivo = Convert.ToInt64(storeprocedure[0][ritorno.Consuntivo]);
                                                Console.WriteLine(controllovita.DescrizioneCompleta);
                                                Console.WriteLine(controllovita.Ultimo_aggiornamento_Consuntivo + "=>" + controllovita.Consuntivo);
                                                _context.Update(controllovita);

                                                if (((controllovita.preallarme_enable) && (controllovita.Consuntivo > controllovita.preallarme_soglia)) || ((controllovita.allarme_enable) && (controllovita.Consuntivo > controllovita.allarme_soglia)))
                                                {
                                                    var CustomUserRole = await _context.CustomUserRole.AsNoTracking().Where(c => c.RoleId == controllovita.Macchina.Nome.Id).ToListAsync(cancellationToken: stoppingToken);
                                                    if (CustomUserRole != null)
                                                        foreach (var userRole in CustomUserRole)
                                                        {
                                                            var user = _context.Users.AsNoTracking().FirstOrDefault(n => n.Id == userRole.UserId);
                                                            if ((controllovita.preallarme_enable)&&(controllovita.Consuntivo > controllovita.preallarme_soglia))
                                                            {
                                                                string resultsms = null;
                                                                await _sendSmsMail.SendSmsEmailTextAsync(user.Email, controllovita.preallarme_testo, controllovita.preallarme_testo + Environment.NewLine + controllovita.DescrizioneCompleta, controllovita.preallarme_testo + "/" + controllovita.DescrizioneBreve, user.PhoneNumber, out resultsms);
                                                                Console.WriteLine("Sms Preallarme:" + resultsms);
                                                            }
                                                            if ((controllovita.allarme_enable) && (controllovita.Consuntivo > controllovita.allarme_soglia))
                                                            {
                                                                string resultsms = null;
                                                                await _sendSmsMail.SendSmsEmailTextAsync(user.Email, controllovita.allarme_testo, controllovita.allarme_testo + Environment.NewLine + controllovita.DescrizioneCompleta, controllovita.allarme_testo + "/" + controllovita.DescrizioneBreve, user.PhoneNumber, out resultsms);
                                                                Console.WriteLine("Sms Allarme:" + resultsms);
                                                            }

                                                            await _context.SaveChangesAsync();
                                                        }
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (controlliVitaNoQuery != null)
                        {
                            foreach (var controllovita in controlliVitaNoQuery)
                            {
                                controllovita.Ultimo_aggiornamento_Consuntivo = Convert.ToDateTime(DateTime.Today);
                                controllovita.Consuntivo = (DateTime.Today - controllovita.Partenza.Value).Days;
                                Console.WriteLine(controllovita.DescrizioneCompleta);
                                Console.WriteLine(controllovita.Ultimo_aggiornamento_Consuntivo + "=>" + controllovita.Consuntivo);
                                _context.Update(controllovita);

                                if (((controllovita.preallarme_enable) && (controllovita.Consuntivo > controllovita.preallarme_soglia)) || ((controllovita.allarme_enable) && (controllovita.Consuntivo > controllovita.allarme_soglia)))
                                {
                                    var CustomUserRole = await _context.CustomUserRole.AsNoTracking().Where(c => c.RoleId == controllovita.Macchina.Nome.Id).ToListAsync(cancellationToken: stoppingToken);
                                    if (CustomUserRole != null)
                                        foreach (var userRole in CustomUserRole)
                                        {
                                            var user = _context.Users.AsNoTracking().FirstOrDefault(n => n.Id == userRole.UserId);
                                            if ((controllovita.preallarme_enable) && (controllovita.Consuntivo > controllovita.preallarme_soglia))
                                            {
                                                string resultsms = null;
                                                await _sendSmsMail.SendSmsEmailTextAsync(user.Email, controllovita.preallarme_testo, controllovita.preallarme_testo + Environment.NewLine + controllovita.DescrizioneCompleta, controllovita.preallarme_testo + "/" + controllovita.DescrizioneBreve, user.PhoneNumber, out resultsms);
                                                Console.WriteLine("Sms Preallarme:" + resultsms);
                                            }
                                            if ((controllovita.allarme_enable) && (controllovita.Consuntivo > controllovita.allarme_soglia))
                                            {
                                                string resultsms = null;
                                                await _sendSmsMail.SendSmsEmailTextAsync(user.Email, controllovita.allarme_testo, controllovita.allarme_testo + Environment.NewLine + controllovita.DescrizioneCompleta, controllovita.allarme_testo + "/" + controllovita.DescrizioneBreve, user.PhoneNumber, out resultsms);
                                                Console.WriteLine("Sms Allarme:" + resultsms);
                                            }

                                            await _context.SaveChangesAsync();
                                        }
                                }
                            }
                        }

                    }
                    else
                    {
                        _logger.LogError(">>>>>>>>>> Database non accessibile");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task.");
                }
                await Task.Delay(TimeSpan.FromMinutes(60 * 24), stoppingToken);
            }

            _logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>MyServiceReader background task is stopping.");
        }

    }
}
