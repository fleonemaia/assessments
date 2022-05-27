using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Graph
{
    public interface IGraph<T>
    {
        IObservable<IEnumerable<T>> RoutesBetween(T source, T target);
    }

    public class Graph<T> : IGraph<T>
    {
        private readonly IEnumerable<ILink<T>> Links;

        public Graph(IEnumerable<ILink<T>> links)
        {
            Links = links;
        }

        public IObservable<IEnumerable<T>> RoutesBetween(T source, T target)
        {
            return Observable.Create<IEnumerable<T>>(observer =>
            {
                var possibleSources = Links.Where(e => e.Source.Equals(source));
                var pathsTaken = new List<T>();

                foreach (var item in possibleSources)
                {
                    var temporarySource = item.Source;
                    var paths = new List<T>();
                    
                    while (!target.Equals(temporarySource))
                    {
                        var nextTarget = Links.Where(e => e.Source.Equals(temporarySource) && !pathsTaken.Contains(e.Target)).FirstOrDefault();
                        paths.Add(temporarySource);
                        pathsTaken.Add(temporarySource);
                        temporarySource = nextTarget.Target;
                    }

                    paths.Add(temporarySource);
                    observer.OnNext(paths);
                }
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}
