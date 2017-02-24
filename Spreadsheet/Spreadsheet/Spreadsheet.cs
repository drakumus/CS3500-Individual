﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formulas;
using Dependencies;
using System.Text.RegularExpressions;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {

        //dictionary for all cells
        private Dictionary<string, Cell> cells;

        //dependency map for all cells
        private DependencyGraph map;

        /// <summary>
        /// Constructs Spreadsheet and initializes its two internal data sets
        /// </summary>
        public Spreadsheet()
        {
            cells = new Dictionary<string, Cell>();
            map = new DependencyGraph();
        }

        /// <summary>
        /// Private Method utilizing Regex to check for names that break convention
        /// </summary>
        /// <param name="name">Cell Name</param>
        /// <returns></returns>
        private bool IsValidName(string name)
        {
            if (Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$", RegexOptions.Singleline) && name != null)
                return true;
            return false;
        }

        /// <summary>
        /// Returns the contents of a given cell after checking for Valid name and null name.
        /// </summary>
        /// <param name="name">Cell Name</param>
        /// <returns></returns>
        public override object GetCellContents(string name)
        {
            if (name == null)
                throw new InvalidNameException();
            if (IsValidName(name) && cells.ContainsKey(name))
            {
                return cells[name].contents;
            }
            else
            {
                throw new InvalidNameException();
            }
            
        }
        /// <summary>
        /// Returns the names of all nonempty cells. This is achieved
        /// by returning all Keys the dictionary currently stores
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return cells.Keys; //returns all logged cells currently stored in the dictionary
        }

        /// <summary>
        /// Sets the contents of a formula based cell.
        /// Throws InvalidName Exception if name is null
        /// Throws ArgumentNullException if formula == null
        /// Throws Circular Dependence Exception if formula is found to contain its own name
        /// 
        /// </summary>
        /// <param name="name"> name of cell</param>
        /// <param name="formula">formula object</param>
        /// <returns></returns>
        public override ISet<string> SetCellContents(string name, Formula formula)
        {
            //valid naming checks/null checks
            if (name == null)
                throw new InvalidNameException();
            if (formula.Equals(null))
                throw new ArgumentNullException();
            //RegexCheck and then assigns cell
            if (IsValidName(name))
            {
                if (cells.ContainsKey(name))
                    cells[name] = new Cell(formula);
                else
                     cells.Add(name, new Cell(formula));
                
            }
            else
                throw new InvalidNameException();

            //CircularException Check
            foreach (string var in formula.GetVariables())
            {
                map.AddDependency(name, var);
                //checks each variable for a match with name which results in a Cirucular Dependency
                if (var == name)
                    throw new CircularException();
            }

            HashSet<string> dependents = new HashSet<string>(map.GetDependees(name));
            dependents.Add(name);

            //returns all the cells effected by the change in a Hashset
            return dependents;
        }

        public override ISet<string> SetCellContents(string name, string text)
        {
            //Exception checks and assignment
            if (name == null)
                throw new InvalidNameException();
            if (text == null)
                throw new ArgumentNullException();
            if (IsValidName(name))
            {
                if (cells.ContainsKey(name))
                    cells[name] = new Cell(text);
                else
                    cells.Add(name, new Cell(text));
            }
            else
                throw new InvalidNameException();

            //Empties Dependies so they can be recalculated
            map.ReplaceDependees(name, new HashSet<string>());

            //returns any dependents of this cell using provided method
            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        public override ISet<string> SetCellContents(string name, double number)
        {
            //exception checks and assignment
            if (name == null)
                throw new InvalidNameException();
            if (number == null)
                throw new ArgumentNullException();
            if (IsValidName(name))
            {
                if (cells.ContainsKey(name))
                    cells[name] = new Cell(number);
                else
                    cells.Add(name, new Cell(number));
            }
            else
                throw new InvalidNameException();

            //same functionality as double SetCellConetnts
            map.ReplaceDependees(name, new HashSet<String>());

            //same functionality as other SetCellConetnts
            return (ISet<string>)map.GetDependents(name);
        }

        /// <summary>
        /// Returns an Enumerable object containing all dependents of a given cell
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            //null checks.
            if (name == null)
                throw new ArgumentNullException();
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }

            //returns dependencies attached to name
            return map.GetDependents(name);
        }
        
        /// <summary>
        /// Private Cell Class used to store values and contents
        /// </summary>
        private class Cell
        {
            public Object contents { get; set; }
            public Object value { get; set; }

            public Cell(string contents)
            {
                this.contents = contents;
                value = "";
            }

            public Cell(Formula contents)
            {
                this.contents = contents;
            }

            public Cell(double contents)
            {
                this.contents = contents;
            }
        }
    }
}