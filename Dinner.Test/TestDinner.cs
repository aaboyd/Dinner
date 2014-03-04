using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nancy;
using Nancy.Testing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Threading.Tasks;

namespace AlexBoyd.Dinner.Test
{
  [TestClass]
  public class TestDinner
  {
    private string _ConnectionString;

    [TestInitialize]
    public void SetupTest()
    {
      //get the full location of the assembly with tests in it
      string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
      string directory = Path.GetDirectoryName(assemblyPath);
      string filePath = Path.Combine(directory, "test.sdf");

      // delete it if it already exists
      if (File.Exists(filePath)) File.Delete(filePath);

      _ConnectionString = "Datasource = " + filePath;

      Database.SetInitializer<MyContext>(new CreateDatabaseIfNotExists<MyContext>());
      using (var context = new MyContext(_ConnectionString))
      {
        context.Database.Create();
      }
    }

    [TestMethod]
    public async Task Test_delete_dinner()
    {
      AlexBoyd.Dinner.Dinner dinner = null;
      using (var ctx = new MyContext(_ConnectionString))
      {
        dinner = ctx.Dinners.Add(new AlexBoyd.Dinner.Dinner()
        {
          Title = "Dinner Title",
          Date = DateTime.UtcNow,
          Address = "Awesome Street, Detroit, MI",
          HostedBy = "The Artist Formerly Known as Prince"
        });

        ctx.SaveChanges();
      }
      var bootstrapper = new ConfigurableBootstrapper(with =>
      {
        with.Module<DinnerModule>();
        with.Dependencies<IMyContext>(new MyContext(_ConnectionString));
      });

      var browser = new Browser(bootstrapper);

      var response = browser.Delete("/dinner/" + Convert.ToString(dinner.Id));
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

      using (var ctx = new MyContext(_ConnectionString))
      {
        int count = await ctx.Set<AlexBoyd.Dinner.Dinner>().CountAsync();
        Assert.AreEqual(0, count);
      }
    }

    [TestMethod]
    public void Test_read_dinner()
    {
      AlexBoyd.Dinner.Dinner dinner = null;
      using (var ctx = new MyContext(_ConnectionString))
      {
        dinner = ctx.Dinners.Add(new AlexBoyd.Dinner.Dinner()
        {
          Title = "Dinner Title",
          Date = DateTime.UtcNow,
          Address = "Awesome Street, Detroit, MI",
          HostedBy = "The Artist Formerly Known as Prince"
        });

        ctx.SaveChanges();
      }
      var bootstrapper = new ConfigurableBootstrapper(with =>
      {
        with.Module<DinnerModule>();
        with.Dependencies<IMyContext>(new MyContext(_ConnectionString));
      });

      var browser = new Browser(bootstrapper);

      var response = browser.Get("/dinner/");

      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

      String responseString = response.Body.AsString();

      IList<AlexBoyd.Dinner.Dinner> dinners = response.Body.DeserializeJson<IList<AlexBoyd.Dinner.Dinner>>();
      Assert.AreEqual(1, dinners.Count);
    }

    [TestMethod]
    public async Task Test_update_dinner()
    {
      AlexBoyd.Dinner.Dinner dinner = null;
      using (var ctx = new MyContext(_ConnectionString))
      {
        dinner = ctx.Dinners.Add(new AlexBoyd.Dinner.Dinner()
        {
          Title = "Dinner Title",
          Date = DateTime.UtcNow,
          Address = "Awesome Street, Detroit, MI",
          HostedBy = "The Artist Formerly Known as Prince"
        });

        ctx.SaveChanges();
      }
      var bootstrapper = new ConfigurableBootstrapper(with =>
      {
        with.Module<DinnerModule>();
        with.Dependencies<IMyContext>(new MyContext(_ConnectionString));
      });

      var browser = new Browser(bootstrapper);
      var newTitle = "I am different";
      var response = browser.Put("/dinner/" + Convert.ToString(dinner.Id), with =>
      {
        with.JsonBody<AlexBoyd.Dinner.Dinner>(new AlexBoyd.Dinner.Dinner()
        {
          Title = newTitle,
          Date = dinner.Date,
          Address = dinner.Address,
          HostedBy = dinner.HostedBy
        });
      });

      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

      using (var ctx = new MyContext(_ConnectionString))
      {
        var updatedDinner = await ctx.Set<AlexBoyd.Dinner.Dinner>().FirstAsync(d => d.Id == dinner.Id);
        Assert.AreEqual(newTitle, updatedDinner.Title);
      }
    }

    [TestMethod]
    public async Task Test_create_dinner()
    {
      var bootstrapper = new ConfigurableBootstrapper(with =>
      {
        with.Module<DinnerModule>();
        with.Dependencies<IMyContext>(new MyContext(_ConnectionString));
      });

      var browser = new Browser(bootstrapper);
      var response = browser.Post("/dinner/", with =>
      {
        with.JsonBody<AlexBoyd.Dinner.Dinner>(new AlexBoyd.Dinner.Dinner()
        {
          Title = "My First Dinner",
          Date = DateTime.UtcNow,
          Address = "1 Campus Martius, Detroit, MI",
          HostedBy = "Alex Boyd"
        });
      });

      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

      using (var ctx = new MyContext(_ConnectionString))
      {
        var count = await ctx.Dinners.CountAsync();
        Assert.AreEqual(1, count);
      }
    }
  }
}
