using EFCoreIssueTest.Models.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreIssueTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            using var mailDB = scope.ServiceProvider.GetService<MailDbContext>(); // ���⿡�� ����ϸ� ������ ����ġ ������ ��Ÿ��.

            #region DB �ʱⵥ���� �Է�
            mailDB.Database.EnsureCreated();
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    mailDB.Add(new ReceiveUser()
                    {
                        Addr = "test@test.com",
                        Name = "Test",
                        Step = 1,
                        TemplateID = 1,
                        UserID = $"uesr00{i}"
                    });
                }
                await mailDB.SaveChangesAsync();
            }
            catch
            {

            }
            #endregion
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var receive in mailDB.ReceiveUser.Where(r => r.Step == 3))
                {
                    // Step ���� 3�� �����Ͱ� ��ȸ�Ǿ� �����ߴµ� ���󺸸� �ʱⰪ�� 1�̴�.
                    // �� ������ �ذ��Ϸ��� ReceiveUser.AsNoTracking().Where(r => r.Step == 3) �� �����ϰų�, mailDB�� while�� ������ �Űܾ��Ѵ�.
                    _logger.LogInformation($"{DateTime.Now:G} receive {receive.UserID} is done. Step:{receive.Step}"); 
                }
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
