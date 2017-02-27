using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formulas;
using Dependencies;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {

        //dictionary for all cells
        private Dictionary<string, Cell> cells;

        //dependency map for all cells
        private DependencyGraph map;

        //private modifier for bool to satisfy protected set.
        private bool changed;

        private Regex isValid;
        /// <summary>
        /// Protected variable to demonstrate a change in spreadsheet.
        /// </summary>
        public override bool Changed
        {
            get
            {
                return changed;
            }

            protected set
            {
                changed = value;
            }
        }

        /// <summary>
        /// Constructs Spreadsheet and initializes its two internal data sets
        /// </summary>
        public Spreadsheet()
        {
            //@"^[a - zA - Z_](?: [a - zA - Z_] |\d)*$"
            //base Spreadsheet
            isValid = new Regex(@".*");

            cells = new Dictionary<string, Cell>();
            map = new DependencyGraph();
            changed = false;
        }

        public Spreadsheet(Regex isValid) : this()
        {
            //regex check
            try
            {
                this.isValid = isValid;
            }
            catch (ArgumentException)
            {
                throw new SpreadsheetReadException("Invalid Regex");
            }
        }
        /// <summary>
        /// Used to read from an xml and set up a spreadsheet acordingly.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newIsValid"></param>
        public Spreadsheet(TextReader source, Regex newIsValid) : this(newIsValid)
        {
            XmlReader reader = XmlReader.Create(source);
            List<string> junk = new List<string>();
            using (reader)
            {
                while (reader.Read())
                {
                //case to set up regex
                    if(reader.Name == "IsValid")
                    {
                        try
                        {
                            newIsValid = new Regex(reader["IsValid"]);
                        }
                        catch
                        {
                            throw new SpreadsheetReadException("Error in Regex");
                        }
                    }
                    else if (reader.Name == "cell")
                    {
                        List<string> temp = new List<string>();

                        //duplicates check
                        foreach (string s in junk)
                        {
                            if (temp.Contains(s))
                                throw new SpreadsheetReadException("duplicate read occured");
                            temp.Add(s);
                        }

                        //adds cells
                        junk.Add(reader["name"]);
                        SetContentsOfCell(reader["name"], reader["contents"]);

                    }/*
                    else if(reader.Name != )
                    {
                        IXmlLineInfo xmlInfo = (IXmlLineInfo)reader;
                        if (xmlInfo.LineNumber != 0)
                            throw new SpreadsheetReadException("Error in XML formatting");
                    }*/
                }
            }
        }
        
        /// <summary>
        /// Private Method utilizing Regex to check for names that break convention
        /// </summary>
        /// <param name="name">Cell Name</param>
        /// <returns></returns>
        private bool IsValidName(string name)
        {
            //if (Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$", RegexOptions.Singleline) && name != null)
            return Regex.IsMatch(name, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*$", RegexOptions.Singleline) && isValid.IsMatch(name);
        }



        /// <summary>
        /// Returns the contents of a given cell after checking for Valid name and null name.
        /// </summary>
        /// <param name="name">Cell Name</param>
        /// <returns></returns>
        public override object GetCellContents(string name)
        {

            //small fix returning cell contents instead of cell name
            if (name == null)
                throw new InvalidNameException();
            //case for setting cell contents of a cell not yet assigned
            if (IsValidName(name))
            {
                if(!cells.ContainsKey(name))
                    return "";
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
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            changed = true;
            //valid naming checks/null checks
            if (name == null)
                throw new InvalidNameException();
            //RegexCheck and then assigns cell

            //IEnumerable<string> oldDependents = map.GetDependents(name);
            //map.ReplaceDependents(name, new HashSet<string>());



            //CircularException Check
            HashSet<string> directDependents = new HashSet<string>(GetCellsToRecalculate(name));

            //update dependents map
            foreach (string var in formula.GetVariables())
            {
                map.AddDependency(name, var);
            }

            //update dependents
            directDependents = new HashSet<string>(GetCellsToRecalculate(name));

            //adds cell to cells
            if (IsValidName(name))
            {
                cells[name] = new Cell(formula);

            }
            else
                throw new InvalidNameException();

            //checks for update in formula value with new variable added
            foreach (string s in GetCellsToRecalculate(name))
            {
                Cell outCell;
                if(cells.TryGetValue(s, out outCell))
                {
                    outCell.EvaluateFormula(t => (double)cells[t].value);
                }
            }

            //returns all the cells effected by the change in a Hashset
            return new HashSet<string>(GetCellsToRecalculate(name));
        }

        /// <summary>
        /// SetCellContents for string
        /// </summary>
        /// <param name="name">cell name</param>
        /// <param name="text">cell string contents</param>
        /// <returns>set of dependent cells</returns>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            changed = true;
            //Exception checks and assignment
            if (name == null)
                throw new InvalidNameException();
            if (text == null)
                throw new ArgumentNullException();
            if (IsValidName(name))
            {
                if (cells.ContainsKey(name))
                {
                    cells[name] = new Cell(text);
                    if(text == "")
                    {
                        cells.Remove(name);
                    }
                }
                else if (text.Length > 0)
                    cells.Add(name, new Cell(text));
            }
            else
                throw new InvalidNameException();

            //Empties Dependies so they can be recalculated
            map.ReplaceDependees(name, new HashSet<string>());
            //checks for update in formula value with new variable added
            foreach (string s in GetCellsToRecalculate(name))
            {
                Cell outCell;
                if (cells.TryGetValue(s, out outCell))
                {
                    outCell.EvaluateFormula(t => (double)cells[t].value);
                }
            }

            //returns any dependents of this cell using provided method
            return new HashSet<string>(GetCellsToRecalculate(name));
        }
        /// <summary>
        /// Sets content of cell for double content based cells
        /// </summary>
        /// <param name="name">cell name</param>
        /// <param name="number">content double</param>
        /// <returns>set of dependents</returns>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            changed = true;
            //exception checks and assignment
            if (name == null)
                throw new InvalidNameException();
            if (number == null)
                throw new ArgumentNullException();
            if (IsValidName(name))
            {
                cells[name] = new Cell(number);
            }
            else
                throw new InvalidNameException();
            /*
            foreach (string s in GetCellsToRecalculate(name))
            {
                Cell outCell;
                if (cells.TryGetValue(s, out outCell))
                {
                    outCell.contents
                    outCell.value = 
                    outCell.EvaluateFormula(LookUp);
                }
            }
            */

            foreach (string s in GetCellsToRecalculate(name))
            {
                Cell outCell;
                if (cells.TryGetValue(s, out outCell))
                {
                    outCell.EvaluateFormula(t => (double)cells[t].value);
                }
            }

            //same functionality as other SetCellConetnts
            //getdependentcells
            return new HashSet<string>(GetCellsToRecalculate(name));
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
            return map.GetDependees(name);
        }

        /// <summary>
        /// Saves the current spreadsheet to an xml
        /// </summary>
        /// <param name="dest"></param>
        public override void Save(TextWriter dest)
        {
            try
            {
                
                XmlWriterSettings set = new XmlWriterSettings();
                set.Indent = true;
                XmlWriter writer = XmlWriter.Create(dest, set);

                string contents;
                changed = false;

                using (writer)
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("IsValid", isValid.ToString());


                    foreach (string cell in cells.Keys)
                    {

                        writer.WriteStartElement("cell");
                        writer.WriteAttributeString("name", cell);


                        if (cells[cell].contents is double)
                        {
                            contents = cells[cell].contents.ToString();
                        }
                        else if (cells[cell].contents is Formula)
                        {
                            contents = "=" + cells[cell].contents.ToString();
                        }
                        else
                        {
                            contents = (string)cells[cell].contents;
                        }


                        writer.WriteAttributeString("contents", contents);
                        writer.WriteEndElement();
                        //dest.WriteLine(tab + "<cell name=" + quote + cell + quote + " contents=" + quote + contents + quote + "></cell>");
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    //dest.WriteLine("</spreadsheet>");
                }
            } catch (IOException)
            {
                throw new SpreadsheetReadException("Invalid content");
            }
        }


        /// <summary>
        /// Returns the stored value of a given cell in the cell set.
        /// </summary>
        /// <param name="name">cell name i.e. A1</param>
        /// <returns>cell.value</returns>
        public override object GetCellValue(string name)
        {
            if (name == null)
                throw new InvalidNameException();
            if (IsValidName(name) && cells.ContainsKey(name))
            {
                return cells[name].value;
            }
            else
            {
                throw new InvalidNameException();
            }
        }

        public override ISet<string> SetContentsOfCell(string name, string content)
        {


            if (content == null)
                throw new ArgumentNullException();
            if (name == null)
                throw new InvalidNameException();

            HashSet<string> dependents;
            double contentDouble; //used to store out double
            string formulaS;

            if (content == "")
            {
                cells.Remove(name);
                return new HashSet<string>(GetCellsToRecalculate(name));
            }
            else if (double.TryParse(content, out contentDouble))
                return SetCellContents(name, contentDouble);
            else if (content.Substring(0, 1) == "=")
            {
                formulaS = content.Substring(1, content.Length - 1);
                Formula tempForm = new Formula(formulaS, s => s.ToUpper(), s => IsValidName(s));

                return SetCellContents(name, new Formula(formulaS));
            }    
            else
                return SetCellContents(name, content);
        }

        /// <summary>
        /// Private Cell Class used to store values and contents
        /// </summary>
        private class Cell
        {
            public Object contents { get; set; }
            public Object value { get; set; }

            public bool FormulaError { get; set; }

            public Cell(string contents)
            {
                this.contents = contents;
                value = contents;
            }

            public Cell(Formula contents)
            {
                this.contents = contents;
                //value = contents.Evaluate();
            }

            public Cell(double contents)
            {
                this.contents = contents;
                value = contents;
            }
            
            public void EvaluateFormula(Lookup lookup)
            {
                if (!(contents is Formula))
                {
                    return;
                }
                
                try
                {
                    Formula form = (Formula)contents;
                    value = form.Evaluate(lookup);
                }catch
                {
                    value = new FormulaError();
                }
                 

            }
            
        }
    }
}
