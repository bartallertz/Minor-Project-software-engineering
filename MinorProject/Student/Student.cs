using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MinorProject.Students
{
    class Student
    {
        public int id { get; set; }

        public string name { get; set; }
        public int age { get; set; }

        public int studentNumber { get; set; }

        public Student(string name, int age, int StudentNumber)
        {
            this.name = name;
            this.age = age;
            this.studentNumber = StudentNumber;
        }

    }

    // select  {s.name, s.studentNumber}.Where(s => s.Age >= 50).OrderBy(s.age)

    

    public static class LinqExpression
    {
        public static string removeEverythingBeforeTheDot(string data){
           string input =data;

         var output = input.Split('.').Last();


        

           return output;
            
        }

        public static string SqlQuerry<a,b,c>(this IEnumerable<a> source, Expression<Func<a,b>> Select, Expression<Func<a,bool>> Where, Expression<Func<a,c>> OrderBy ){
            
            var select2 = select(Select);
            var where2 = where(Where);
            var orderby2 = orderby(OrderBy);

            return select2 + " " +  where2 + " "+ orderby2;            
        }

      

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string select<T, U>(Expression<Func<T, U>> expression)
        {
           
            var st = new StackTrace();

            var sf = st.GetFrame(0);

            var select = sf.GetMethod().Name + " ";            

            ParameterExpression pe = Expression.Parameter(typeof(T), "s");


            var type = typeof(T).ToString();

            //Porject.Student.Student

            var from = removeEverythingBeforeTheDot(type);

            

            var expressionTree = Expression.Lambda<Func<T,U>>(expression.Body, new[] { pe });

            var result = select +  expressionTree.Body;

            return result +  " from " + from;
        }
        //Func<T, bool> isAdult = s => s.age >= 18;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string where<T>(Expression<Func<T, bool>> expression)
        {
            var st = new StackTrace();

            var sf = st.GetFrame(0);

            var Where = sf.GetMethod().Name + " ";

            var body = expression.Body;

            ParameterExpression pe = Expression.Parameter(typeof(T), "s");

            var ExpressionTree = Expression.Lambda<Func<T, bool>>(body, new[] { pe });

            var result = Where + ExpressionTree.Body;

            return result;

        }
        public static string? orderby<T, U>(Expression<Func<T, U>> expression)
        {


            var st = new StackTrace();

            var sf = st.GetFrame(0);

            var OrderBy = sf.GetMethod().Name + " ";

            var _orderBy = OrderBy.Replace("orderby", "Order By");


            var body = expression.Body;

            ParameterExpression pe = Expression.Parameter(typeof(T), "s");

            var expressionTree = Expression.Lambda<Func<T, U>>(body, new[] { pe });



            return _orderBy + expressionTree.Body.ToString();

        }
    }
}
