using System.Linq;

using Nancy;
using Nancy.ModelBinding;
using System.Data.Entity;

namespace AlexBoyd.Dinner
{
  public class DinnerModule : NancyModule
  {
    public DinnerModule(IMyContext ctx) : base("/dinner")
    {
      Get["/"] = x =>
      {
        return Response.AsJson<object>(ctx.Dinners.ToArray());
      };

      Post["/"] = _ =>
      {
        Dinner dinner = this.Bind<Dinner>();

        ctx.Dinners.Add(dinner);
        ctx.SaveChanges();
        
        return HttpStatusCode.OK;
      };

      Put["/{id:int}"] = parameters =>
      {
        Dinner dinner = this.Bind<Dinner>();

        dinner.Id = parameters.id;
        ctx.Dinners.Attach(dinner);
        ctx.Entry(dinner).State = EntityState.Modified;
        ctx.SaveChanges();

        return HttpStatusCode.OK;
      };

      Delete["/{id:int}"] = x =>
      {
        var dinner = new Dinner() { Id = x.id };
        ctx.Entry(dinner).State = EntityState.Deleted;

        ctx.SaveChanges();

        return HttpStatusCode.OK;
      };
    }
  }
}