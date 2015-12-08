using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using ININ.IceLib.Configuration;
using ININ.IceLib.Configuration.Mailbox;
using ININ.IceLib.Connection;
using ININ.IceLib.Statistics;
using ININ.IceLib.Statistics.Alerts;

namespace DataGenerator
{
    class Program
    {
        private static readonly Session _session = new Session();
        private static Random _random = new Random();


        static void Main(string[] args)
        {
            try
            {
                Console.Write("CIC Server: ");
                var server = Console.ReadLine();
                Console.Write("CIC username: ");
                var username = Console.ReadLine();
                Console.Write("CIC password: ");
                var password = Console.ReadLine();

                Console.WriteLine("Connecting...");
                _session.Connect(new SessionSettings(),
                    new HostSettings(new HostEndpoint(server)),
                    new ICAuthSettings(username, password),
                    new StationlessSettings());
                Console.WriteLine("Connected to {0}", _session.Endpoint.Host);

                if (args.Contains("/users"))
                    CreateUsers(100);

                if (args.Contains("/alerts"))
                {
                    CreateAlerts(100);
                }

                if (args.Length == 0)
                {
                    Console.WriteLine("1 - Create users");
                    Console.WriteLine("2 - Create alerts");
                    var key = Console.ReadLine();
                    Console.WriteLine();
                    if (key == "1")
                    {
                        Console.WriteLine("How many users?");
                        var numString = Console.ReadLine();
                        var num = 0;
                        if (int.TryParse(numString, out num))
                            CreateUsers(num);
                        else
                            Console.WriteLine("Dude, that's not a number!");
                    }
                    else if (key == "2")
                    {
                        Console.WriteLine("Create alerts for how many users?");
                        var numString = Console.ReadLine();
                        var num = 0;
                        if (int.TryParse(numString, out num))
                            CreateAlerts(num);
                        else
                            Console.WriteLine("Dude, that's not a number!");
                    }
                    else
                    {
                        Console.WriteLine("whatever dude...");
                    }
                }

                Console.WriteLine("Application finished. Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void CreateUsers(int count)
        {
            var userConfigurationList = new UserConfigurationList(ConfigurationManager.GetInstance(_session));

            for (var i = 0; i < count; i++)
            {
                var sw = new Stopwatch();
                sw.Start();
                var user = userConfigurationList.CreateObject();
                user.PrepareForEdit();
                user.SetConfigurationId("test_" + i);
                user.Mailbox.ApplyMailboxSettings(new NoMailboxSettings("Test " + i));
                user.Roles.Value.Add(new ConfigurationId("Administrator"));
                user.Commit();
                sw.Stop();
                Console.WriteLine("Created user: test_" + i + " in " + sw.ElapsedMilliseconds + "ms");
            }
        }

        private static void CreateAlerts(int userCount)
        {
            for (var i = 0; i < userCount; i++)
            {
                var username = "test_" + i;
                var alertCatalog = new AlertCatalog(StatisticsManager.GetInstance(_session));
                var alertSet = new EditableAlertSet
                {
                    DisplayString = "Alert set for " + username,
                    AccessMode = AlertSetAccessMode.Owned,
                    Description = "Created by DataGenerator at " + DateTime.Now.ToLongTimeString()
                };

                if (_random.Next(1, 1000) <= 300)
                {
                    alertSet.AddAlertDefinition(
                        CreateAlertDefinition(
                            "inin.workgroup:InteractionsWaiting?ININ.People.WorkgroupStats:Workgroup=Sales", alertSet,
                            username, AlertSeverity.Critical));
                    alertSet.AddAlertDefinition(
                        CreateAlertDefinition(
                            "inin.workgroup:InteractionsWaiting?ININ.People.WorkgroupStats:Workgroup=Support", alertSet,
                            username, AlertSeverity.Warning));
                    alertSet.AddAlertDefinition(
                        CreateAlertDefinition(
                            "inin.workgroup:InteractionsWaiting?ININ.People.WorkgroupStats:Workgroup=Marketing",
                            alertSet,
                            username, AlertSeverity.Major));
                    alertSet.AddAlertDefinition(
                        CreateAlertDefinition(
                            "inin.workgroup:InteractionsWaiting?ININ.People.WorkgroupStats:Workgroup=Chat", alertSet,
                            username, AlertSeverity.Minor));
                }

                alertCatalog.CreateAlertSet(alertSet);

                var alertSet2 = new EditableAlertSet(EditableAlertSetUpdateOperation.Modify, alertSet);
                alertSet2.Owner = username;
                alertCatalog.UpdateAlertSet(alertSet2);

                Console.WriteLine((alertSet.AlertDefinitions.Count > 0 ? "*" : "") + "Created alerts for " + username);
            }
        }

        private static AlertDefinition CreateAlertDefinition(string statisticUri, AlertSet alertSet, string username,
            AlertSeverity severity)
        {
            var statkey =
                    new StatisticKeyTemplate(statisticUri);
            var definition = new AlertDefinition(alertSet,
                new StatisticKey(statkey.StatisticIdentifier, statkey.Parameters));
            var action = new AlertAction(username);

            var rule = new AlertRule(definition, severity);
            rule.Condition = StringCondition.CreateContainsCondition(_random.Next(1,10000).ToString());
            rule.AddAlertAction(action);

            definition.AddAlertRule(rule);

            return definition;
        }
    }
}
