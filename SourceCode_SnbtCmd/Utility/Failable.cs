using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TryashtarUtils.Utility
{
    public static class FailableFactory
    {
        public static IFailable Failure(Exception exc, string description)
        {
            return new Failable(exc, description);
        }

        public static IFailable AggregateFailure(params Exception[] exceptions)
        {
            return Aggregate(exceptions.Select(x => Failure(x, null)).ToArray());
        }

        public static IFailable<T> Success<T>(T result, string description)
        {
            return new Failable<T>(result, null, description);
        }

        public static IFailable<T> Aggregate<T>(params IFailable<T>[] failures)
        {
            var flattened = failures.SelectMany(x => x.Flattened()).ToList();
            var exception = new AggregateException(flattened.Select(x => x.Exception));
            string description = String.Join(Environment.NewLine, flattened.Select(x => x.Description));
            return new Failable<T>(default, exception, description, flattened);
        }

        public static IFailable Aggregate(params IFailable[] failures)
        {
            var flattened = failures.SelectMany(x => x.Flattened()).ToList();
            var exception = new AggregateException(flattened.Select(x => x.Exception));
            string description = String.Join(Environment.NewLine, flattened.Select(x => x.Description));
            return new Failable(exception, description, flattened);
        }
    }

    public interface IFailable
    {
        string ToStringSimple();
        string ToStringDetailed();
        string Description { get; }
        Exception Exception { get; }
        bool Failed { get; }
        IEnumerable<IFailable> Flattened();
    }

    public interface IFailable<out T> : IFailable
    {
        new IEnumerable<IFailable<T>> Flattened();
        T Result { get; }
        IFailable<U> Then<U>(Func<T, U> further);
        IFailable<U> Cast<U>();
    }

    public abstract class AbstractFailable : IFailable
    {
        protected Exception _Exception;
        public Exception Exception => _Exception;
        protected string _Description;
        public string Description => _Description;
        public bool Failed => _Exception != null;
        protected IFailable[] Subfailures;
        public bool IsAggregate => Subfailures.Any();

        protected AbstractFailable()
        {
            Subfailures = Array.Empty<IFailable>();
        }

        protected AbstractFailable(Exception exc, string description) : this(exc, description, Array.Empty<IFailable>()) { }
        protected AbstractFailable(Exception exc, string description, IEnumerable<IFailable> nested)
        {
            _Exception = exc;
            _Description = description;
            Subfailures = nested.ToArray();
        }

        public string ToStringSimple()
        {
            if (IsAggregate)
            {
                var messages = new HashSet<string>();
                var summaries = new List<string>();
                foreach (var item in Subfailures)
                {
                    if (messages.Add(item.Exception.Message))
                        summaries.Add(item.ToStringSimple());
                }
                return String.Join(Environment.NewLine, summaries);
            }
            else
            {
                if (Failed)
                    return ExceptionMessage(Exception);
                else
                    return $"{Description}: Operation succeeded";
            }
        }

        public string ToStringDetailed()
        {
            if (IsAggregate)
                return String.Join(Environment.NewLine + Environment.NewLine, Subfailures.Select(x => x.ToStringDetailed()));
            else
            {
                if (Failed)
                    return $"{Description}:{Environment.NewLine}{Exception}";
                else
                    return $"{Description}: Operation succeeded";
            }
        }

        public IEnumerable<IFailable> Flattened()
        {
            if (IsAggregate)
                return Subfailures;
            return new[] { this };
        }

        protected static string ExceptionMessage(Exception exception)
        {
            string message = exception.Message;
            if (exception is AggregateException aggregate)
            {
                if (aggregate.InnerExceptions.Count == 1)
                    return ExceptionMessage(aggregate.InnerExceptions[0]);
                message += Environment.NewLine + String.Join(Environment.NewLine, aggregate.InnerExceptions.Select(ExceptionMessage));
            }
            else
            {
                if (exception is WebException web && web.Response != null)
                {
                    using var reader = new StreamReader(web.Response.GetResponseStream());
                    message += Environment.NewLine + reader.ReadToEnd();
                }
                if (exception.InnerException != null)
                    message += Environment.NewLine + ExceptionMessage(exception.InnerException);
            }
            return message;
        }
    }

    public class Failable : AbstractFailable
    {
        public Failable(Action operation, string description)
        {
            _Description = description;
            try { operation(); }
            catch (Exception ex) { _Exception = ex; }
        }

        public Failable(Exception exc, string description, IEnumerable<IFailable> subfailures) : base(exc, description, subfailures) { }
        public Failable(Exception exc, string description) : base(exc, description) { }
    }

    public class Failable<T> : AbstractFailable, IFailable<T>
    {
        private readonly T _Result;
        public T Result
        {
            get
            {
                if (Failed)
                    throw Exception;
                return _Result;
            }
        }

        public IFailable<U> Then<U>(Func<T, U> further)
        {
            if (Failed)
                return Cast<U>();
            return new Failable<U>(() => further(this.Result), this.Description);
        }

        public IFailable<U> Cast<U>()
        {
            var result = this.Failed ? default : (U)(object)this.Result;
            return new Failable<U>(result, this.Exception, this.Description, this.Subfailures.Cast<IFailable<T>>().Select(x => x.Cast<U>()));
        }

        public Failable(Func<T> operation, string description)
        {
            _Description = description;
            try
            { _Result = operation(); }
            catch (Exception ex)
            { _Exception = ex; }
        }

        public Failable(T result, Exception exc, string description, IEnumerable<IFailable<T>> subfailures) : base(exc, description, subfailures)
        {
            _Result = result;
        }

        public Failable(T result, Exception exc, string description) : base(exc, description)
        {
            _Result = result;
        }

        IEnumerable<IFailable<T>> IFailable<T>.Flattened()
        {
            return base.Flattened().Cast<IFailable<T>>();
        }
    }
}
