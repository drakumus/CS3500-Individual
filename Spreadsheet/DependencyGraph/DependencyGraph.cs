// Skeleton implementation written by Joe Zachary for CS 3500, January 2017.
// Interpretation coded by Rohan Cheeniyil u0914584

using System;
using System.Collections.Generic;

namespace Dependencies
{
    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if  and only if 
    /// s1 equals s2 and t1 equals t2.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that the dependency (s,t) is in DG 
    ///    is called the dependents of s, which we will denote as dependents(s).
    ///        
    ///    (2) If t is a string, the set of all strings s such that the dependency (s,t) is in DG 
    ///    is called the dependees of t, which we will denote as dependees(t).
    ///    
    /// The notations dependents(s) and dependees(s) are used in the specification of the methods of this class.
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    ///     
    /// All of the methods below require their string parameters to be non-null.  This means that 
    /// the behavior of the method is undefined when a string parameter is null.  
    ///
    /// IMPORTANT IMPLEMENTATION NOTE
    /// 
    /// The simplest way to describe a DependencyGraph and its methods is as a set of dependencies, 
    /// as discussed above.
    /// 
    /// However, physically representing a DependencyGraph as, say, a set of ordered pairs will not
    /// yield an acceptably efficient representation.  DO NOT USE SUCH A REPRESENTATION.
    /// 
    /// You'll need to be more clever than that.  Design a representation that is both easy to work
    /// with as well acceptably efficient according to the guidelines in the PS3 writeup. Some of
    /// the test cases with which you will be graded will create massive DependencyGraphs.  If you
    /// build an inefficient DependencyGraph this week, you will be regretting it for the next month.
    /// </summary>
    public class DependencyGraph
    {
        //hashmap/dictionary based around defining dependees attached to each dependent. This data structure was chosen
        //because I found it to be the best representation of Directed Acyclic Graphs with
        //a logical structure for the tasks required by Dependency Graph.
        //HashSet is used over list due to its ability to prevent duplicates and having a return on Adds and Removes
        private Dictionary<string, HashSet<string>> dependents;
        //dependees is a Dictionary built at the same time as dependents to remove the requirements for expensive
        //algorithms that would otherwise have to quadratically step through a two dementional data structure.
        //The result is a big O (2n) efficiency instead of n^2. Dependees maps dependents to a dependee.
        private Dictionary<string, HashSet<string>> dependees;

        private int size;

        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        /// 
        public DependencyGraph() : this(null)
        {
            //initialization for the above data structures.
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();
            size = 0;
        }

        public DependencyGraph(DependencyGraph graph) 
        {
            this.dependees = graph.dependees;
            this.dependents = graph.dependents;
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get
            {
                //value of size is adjusted in the methods Add and Remove which are the only two methods 
                //that directly deal with size adjustment.
                return size;
            }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependents(string s)
        {
            //checks if paramter is null.
            if (s == null)
            {
                throw new ArgumentNullException("The parameter passed is null.");
            }
            //has dependents checks that the dictionary for dependees has dependences mapped to the string s.
            return dependees.ContainsKey(s);
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependees(string s)
        {
            //checks if paramter is null.
            if (s == null)
            {
                throw new ArgumentNullException("The parameter passed is null.");
            }
            //similar to HasDependents, Has Dependees checks the dictionary for dependents to see if there are any dependees mapped to it.
            return dependents.ContainsKey(s);
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            //checks if paramter is null.
            if (s == null)
            {
                throw new ArgumentNullException("The parameter passed is null.");
            }
            //checks if the provided string actually has dependents. If it doesn't an empty HashSet is returned
            //(as shown in the guidelines commented above). Otherwise an itterable object is returned contaning the respective dependents
            //mapped to a dependee.
            if (HasDependents(s))
                return new HashSet<string>(dependees[s]);
            return new HashSet<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            //checks if paramter is null.
            if (s == null)
            {
                throw new ArgumentNullException("The parameter passed is null.");
            }
            //again checks to see if a given string has dependees. If it does then the dependees mapped to the dependent are returend
            //in an iterable object. If not then a black hashset is returned per example output above.
            if (HasDependees(s))
                return new HashSet<string>(dependents[s]);
            return new HashSet<string>();

        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            //checks if paramter is null.
            if (s == null || t == null)
                throw new ArgumentNullException("The parameter passed is null");
            //case to check if the library already has the key for dependents.
            if (dependees.ContainsKey(s))
            {
                //if so it attempts to add the key.
                //the beauty of HashSets is that not only do they prevent duplications but they also return true or false
                //if an add or remove is successful. Due to thie capability I'm able to incrememnt size using just the conditional
                //add here.
                if (dependees[s].Add(t))
                {
                    size++;
                }
            }
            else
            {
                //if the container does not have the key then the dependent is added
                size += 1;
                dependees.Add(s, new HashSet<string>() { t });

            }

            //checks if a dependee exists in the dependent's map
            if (dependents.ContainsKey(t))
                //this statement is not turned into a conditional due to the size already being managed.
                dependents[t].Add(s);
            else
            {
                //adds a new dependee to the dependents map.
                dependents.Add(t, new HashSet<string>() { s });

            }

        }

        /// <summary>
        /// This simple function is used to remove dependent node 1
        /// and dependee node 2 from a dictionary and is only called
        /// after remove. It was made into a seperate method for ease
        /// of use in the future and documentation purposes.
        /// </summary>
        private void cleanDictionary(string node1, string node2)
        {
            //checks if paramter is null.
            if (node1 == null || node2 == null)
                throw new ArgumentNullException("The parameter passed is null");
            if (dependees[node1].Count == 0)
                dependees.Remove(node1);
            if (dependents[node2].Count == 0)
                dependents.Remove(node2);
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            //checks if paramter is null.
            if (s == null || t == null)
                throw new ArgumentNullException("The parameter passed is null");
            //checks if a provided dependent actually exists. (s != null)
            if (dependees.ContainsKey(s))
            {
                //conditional for if the provided dependee exists. (t != null)
                if (dependees[s].Remove(t))
                {
                    //handles size if the item is successfully removed.
                    size--;
                    //if a dependent lookup is successful it can be assumed a dependee
                    //lookup will also be successful removing the need to code it in.
                    //Thus the dependee entree s is removed from t.
                    dependents[t].Remove(s);
                    //checks if s maps any dependents and if t maps any dependees.
                    cleanDictionary(s, t);
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            //checks if paramter is null.
            if (s == null)
                throw new ArgumentNullException("The parameter passed is null");
            if (HasDependents(s))
            {
                //instance variable created so the itterated object doesn't experience edits
                //while itering.
                IEnumerable<string> depend = GetDependents(s);
                //removes dependents and dependees.
                foreach (string t in depend)
                {
                    if (t == null)
                        throw new ArgumentNullException("One of the strings in the dependencies is null");
                    RemoveDependency(s, t);
                }
                //adds new dependents and updates dependees.
                foreach (string t in newDependents)
                {
                    if (t == null)
                        throw new ArgumentNullException("One of the strings in the new dependencies is null");
                    AddDependency(s, t);
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            if (HasDependees(t))
            {
                //again instance variable provided to prevent iteration errors.
                IEnumerable<string> depend = GetDependees(t);
                //removes dependees and dependents.
                foreach (string r in depend)
                {
                    //checks if one of the dependencies is null.
                    if (t == null)
                        throw new ArgumentNullException("One of the strings in the dependencies is null");
                    RemoveDependency(r, t);
                }
                //adds new dependees and updates dependents.
                foreach (string s in newDependees)
                {
                    //checks if one of the dependencies is null.
                    if (t == null)
                        throw new ArgumentNullException("One of the strings in the new dependencies is null");
                    AddDependency(s, t);
                }
            }
        }
    }
}