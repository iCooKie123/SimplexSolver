using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace SimplexSolver.Pages
{
    partial class Index
    {
        static int variabile = 3;
        static int restrictii = 3;
        string SelectedMode = "max";
        private bool tabelVisible = false;
        private bool inputDisabled = true;
        private string ButtonText => tabelVisible ? "Sterge tabel" : "Genereaza tabel";
        private double[][] matrix;
        private double[] functionValues;
        private double[] restrictionValues;
        private string inputValue;
        private static string? errorVariables => variabile < 2 ? "Numarul de variabile trebuie sa fie mai mare sau egal cu 2" : variabile > 10 ? "Numarul de variabile trebuie sa fie mai mic sau egal cu 10" : null;
        private static string? errorRestrictii => restrictii < 2 ? "Numarul de restrictii trebuie sa fie mai mare sau egal cu 2" : restrictii > 10 ? "Numarul de restrictii trebuie sa fie mai mic sau egal cu 10" : null;

        private List<SimplexStep> simplexSteps;
        private void genereazaTabel()
        {
            if (tabelVisible)
            {
                stergeTabel();
                return;
            }
            if (variabile < 2 || variabile > 10 || restrictii < 2 || restrictii > 10)
            {
                return;
            }

            matrix = new double[restrictii][];
            for (int i = 0; i < restrictii; i++)
            {
                matrix[i] = new double[variabile];
            }

            functionValues = new double[variabile];
            restrictionValues = new double[restrictii];

            tabelVisible = true;
            StateHasChanged();
        }

        private void stergeTabel()
        {
            matrix = null;
            functionValues = null;
            restrictionValues = null;
            tabelVisible = false;
            simplexSteps = null;
            StateHasChanged();
        }

        private void solutiePropusa()
        {
            if (tabelVisible)
            {
                stergeTabel();
            }
            variabile = 3;
            restrictii = 3;
            genereazaTabel();
            functionValues = new double[] { 1, 1, 1 };
            restrictionValues = new double[] { 1, 1, 1 };
            matrix[0][0] = 3;
            matrix[0][1] = 2;
            matrix[0][2] = 3;
            matrix[1][0] = 2;
            matrix[1][1] = 4;
            matrix[1][2] = 1;
            matrix[2][0] = 1;
            matrix[2][1] = 3;
            matrix[2][2] = 2;
            OnCheckButtonClicked();
            StateHasChanged();
        }


        private void OnCheckButtonClicked()
        {
            // Perform simplex algorithm calculations and store the steps
            simplexSteps = PerformSimplexAlgorithm();
        }

        private void HandleInputRestricti(ChangeEventArgs e)
        {
            var value = e.Value.ToString();
            if (Int32.Parse(value) >= 2)
            {

                restrictii = Int32.Parse(value);
            }
            else
            {
                restrictii = 3;
            }
            if (tabelVisible) genereazaTabel();
        }
        private void HandleInputVariabile(ChangeEventArgs e)
        {
            int value;
            if (int.TryParse(e.Value.ToString(), out value))
            {
                if (value < 3)
                {
                    variabile = 3;
                }
                else if (value > 10)
                {
                    variabile = 10;
                }
                else
                {
                    variabile = value;
                }
            }
            else
            {
                variabile = 3;
            }
            if (tabelVisible) genereazaTabel();
        }

        private void CustomFunction(ChangeEventArgs e, int i, int j)
        {
            int value;
            if (int.TryParse(e.Value.ToString(), out value))
            {
                matrix[i][j] = value;
            }

        }

        private void CustomFunction2(ChangeEventArgs e, int row, double value)
        {
            if (double.TryParse(e.Value.ToString(), out double newValue))
            {
                value = newValue; // Update the temporary variable with the modified value
            }

            functionValues[row] = value; // Update the array with the modified value
        }

        private void CustomFunction3(ChangeEventArgs e, int i)
        {
            int value;
            if (int.TryParse(e.Value.ToString(), out value))
            {
                restrictionValues[i] = value; // Update the array with the modified value
            }
        }

        private List<SimplexStep> PerformSimplexAlgorithm()
        {
            Console.WriteLine("start");
            var steps = new List<SimplexStep>();

            // Validate sizes of arrays
            if (matrix.Length != restrictii || functionValues.Length != variabile || restrictionValues.Length != restrictii)
            {
                return steps; // Return empty steps list if sizes are inconsistent
            }

            // Step 0: Initial table setup
            var table = new double[restrictii][];
            var indexOfOne = variabile;
            for (int i = 0; i < restrictii; i++)
            {
                table[i] = new double[variabile + restrictii];
                for (int j = 0; j < variabile + restrictii; j++)
                {
                    if (j < variabile)
                    {
                        table[i][j] = matrix[i][j];
                    }
                    else
                    {
                        if (j == indexOfOne)
                        {
                            table[i][j] = 1;
                        }
                        else
                        {
                            table[i][j] = 0;
                        }

                    }
                }
                indexOfOne++;
            } //this is correct


            var xb = (double[])restrictionValues.Clone();

            //calculam baza si coef bazei
            double[] baza = new double[restrictii];
            var primaBaza = variabile;
            for (int i = 0; i < restrictii; i++)
            {
                baza[i] = primaBaza;
                primaBaza++;
            }
            Console.WriteLine($"baza: {JsonSerializer.Serialize(baza)}");
            double[] coefBazeiInit = new double[restrictii + variabile];
            for (int i = 0; i < restrictii + variabile; i++)
            {
                if (i < restrictii)
                {
                    coefBazeiInit[i] = functionValues[i];
                }
                else
                {
                    coefBazeiInit[i] = 0;
                }
            }
            Console.WriteLine($"Coef bazei init: {JsonSerializer.Serialize(coefBazeiInit)}");
            double[] coefBazei = baza.Select(i => coefBazeiInit[(int)i]).ToArray();
            Console.WriteLine($"Coef bazei calculat : {JsonSerializer.Serialize(coefBazei)}");
            var zj = CalculateZj(table, coefBazei);
            Console.WriteLine($"ZJ CALCULAT: {JsonSerializer.Serialize(zj)}");
            var deltaj = CalculateDeltaJ(coefBazeiInit, zj);
            Console.WriteLine($"dj CALCULAT: {JsonSerializer.Serialize(CalculateDeltaJ(coefBazeiInit, zj))}");
            //check if the problem is optimal

            int iterations = 0;
            while (true)
            {
                Console.WriteLine($"optim:{IsOptimal(deltaj)}");
                if (IsOptimal(deltaj) || iterations > 100) break;
                Console.WriteLine($"iteratia {iterations}");
                var pivotElementColumn = FindEnteringColumn(deltaj);
                Console.WriteLine(JsonSerializer.Serialize(table));
                var pivotElementRow = FindExitingColumn(xb, table, pivotElementColumn);
                Console.WriteLine(JsonSerializer.Serialize(table));
                var pivotElement = table[pivotElementRow][pivotElementColumn];
                Console.WriteLine($"pivot: {pivotElement}");
                Console.WriteLine(JsonSerializer.Serialize(table));
                Console.WriteLine($"xb inainte de hau hau:{JsonSerializer.Serialize(xb)}");
                var tableCopy = (double[][])table.Clone();
                var observatii = new List<string>();
                observatii.Add($" C↓B : {(SelectedMode == "max" ? "Max" : "Min")} Δj = Δ{pivotElementColumn + 1} = {DoubleToFractionString(deltaj[pivotElementColumn])}");
                var minimulString = baza[pivotElementRow] < variabile ? $"X{baza[pivotElementRow] + 1}" : $"S{baza[pivotElementRow] - variabile + 1}";
                observatii.Add($" C↑B : Min Xb/X=ak = {DoubleToFractionString(xb[pivotElementColumn]/table[pivotElementRow][pivotElementColumn])} care corespunde lui {minimulString}");
                observatii.Add($" Pivot: {DoubleToFractionString(tableCopy[pivotElementRow][pivotElementColumn])}");
                var zk = CalculateZk(coefBazei, xb);
                Console.WriteLine(JsonSerializer.Serialize(observatii));
                steps.Add(new SimplexStep
                {
                    StepNumber = iterations,
                    TableHeaders = GenerateTableHeaders(),
                    TableRows = GenerateTableRows(tableCopy, baza, coefBazei, xb),
                    DeltaJ = DoubleToFractionString(deltaj),
                    PivotElement = DoubleToFractionString(tableCopy[pivotElementRow][pivotElementColumn]),
                    XB = xb.Select(x => x.ToString()).ToArray(),
                    ZJ = zj.Select(z => z.ToString()).ToArray(),
                    CB = coefBazei.Select(c => c.ToString()).ToArray(),
                    Baza = baza.Select(b => b.ToString()).ToArray(),
                    ZK = DoubleToFractionString(zk),
                    Observatii = observatii,
                    PivotColumn = pivotElementColumn+3,
                    PivotRow = pivotElementRow

                });
                PerformTableTransformation(table, pivotElementColumn, pivotElementRow, xb);
                // calculam baza si coef bazei
                Console.WriteLine($"xb dupa de hau hau:{JsonSerializer.Serialize(xb)}");
                baza[pivotElementRow] = pivotElementColumn;
                Console.WriteLine($"baza: {JsonSerializer.Serialize(baza)}");
                coefBazei = baza.Select(i => coefBazeiInit[(int)i]).ToArray();
                Console.WriteLine($"Coef bazei calculat : {JsonSerializer.Serialize(coefBazei)}");
                zj = CalculateZj(table, coefBazei);
                Console.WriteLine($"ZJ CALCULAT: {JsonSerializer.Serialize(zj)}");
                deltaj = CalculateDeltaJ(coefBazeiInit, zj);
                Console.WriteLine($"dj CALCULAT: {JsonSerializer.Serialize(CalculateDeltaJ(coefBazeiInit, zj))}");
                iterations++;
            }
            var observatiiFinale = new List<string>();
            observatiiFinale.Add($"Solutia problemei este: ");
            for (int i = 0; i < variabile; i++)
            {
                var index = baza.ToList().IndexOf(i);
                if (index != -1)
                {
                    observatiiFinale.Add($"X{i + 1} = {DoubleToFractionString(xb[index])}");
                }
                else
                {
                    observatiiFinale.Add($"X{i + 1} = 0");
                }
            }
            observatiiFinale.Add($"Z = {DoubleToFractionString(CalculateZk(coefBazei, xb))}");
            steps.Add(new SimplexStep
            {
                StepNumber = iterations,
                TableHeaders = GenerateTableHeaders(),
                TableRows = GenerateTableRows(table, baza, coefBazei, xb),
                DeltaJ = DoubleToFractionString(deltaj),
                PivotElement = "Optimal",
                Observatii = observatiiFinale,
                ZK = DoubleToFractionString(CalculateZk(coefBazei, xb)),
                PivotColumn = Int32.MaxValue,
                PivotRow = Int32.MaxValue
            });

            return steps;
        }

        private void PerformTableTransformation(double[][] table, int columnIndexOfPivot, int rowIndexOfPivot, double[] xb)
        {
            var pivotElement = table[rowIndexOfPivot][columnIndexOfPivot];
            var pivotRow = table[rowIndexOfPivot];

            // Divide pivot row by pivot element
            xb[rowIndexOfPivot] /= pivotElement;
            for (int i = 0; i < pivotRow.Length; i++)
            {
                pivotRow[i] /= pivotElement;
            }
            // Subtract pivot row from all other rows
            for (int i = 0; i < table.Length; i++)
            {
                if (i == rowIndexOfPivot)
                    continue;
                var row = table[i];
                var rowMultiplier = row[columnIndexOfPivot];
                for (int j = 0; j < row.Length; j++)
                {
                    row[j] -= rowMultiplier * pivotRow[j];
                }
                xb[i] -= rowMultiplier * xb[rowIndexOfPivot];
            }
        }


        private int FindEnteringColumn(double[] deltaJ)
        {
            int index;
            if (SelectedMode == "max") index = Array.IndexOf(deltaJ, deltaJ.Max());
            else index = Array.IndexOf(deltaJ, deltaJ.Min());
            return index;
        }

        private int FindExitingColumn(double[] xb, double[][] table, int enteringColumn)
        {
            // we create a copy of the table array
            var tableCopy = new double[table.Length][];
            for (int i = 0; i < table.Length; i++)
            {
                tableCopy[i] = new double[table[i].Length];
                for (int j = 0; j < table[i].Length; j++)
                {
                    tableCopy[i][j] = table[i][j];
                }
            }
            Console.WriteLine($"copy:{JsonSerializer.Serialize(tableCopy)}");
            // we calculate the ratios
            var ratios = new double[tableCopy.Length];
            for (int i = 0; i < xb.Length; i++)
            {
                ratios[i] = xb[i] / tableCopy[i][enteringColumn];
                if (ratios[i] < 0)
                {
                    ratios[i] = double.MaxValue;
                }
            }
            Console.WriteLine($"ratios:{JsonSerializer.Serialize(ratios)}");

            var indexOfMinRatio = Array.IndexOf(ratios, ratios.Min());

            Console.WriteLine($"minRatio:{ratios[indexOfMinRatio]}");
            Console.WriteLine($"minRatioIndex:{indexOfMinRatio}");
            return indexOfMinRatio;
        }

        private bool IsOptimal(double[] deltaJ)
        {
            bool optimal;
            if (SelectedMode == "max") optimal = deltaJ.All(d => d <= 0);
            else optimal = deltaJ.All(d => d >= 0);
            return optimal;
        }

        private double CalculateZk(double[] cb, double[] xb)
        {
            double zk = 0;
            for (int i = 0; i < cb.Length; i++)
            {
                zk += cb[i] * xb[i];
            }
            return zk;
        }

        private double[] CalculateDeltaJ(double[] cbInit, double[] zj)
        {
            var deltaJ = new double[variabile + restrictii];
            for (int j = 0; j < variabile + restrictii; j++)
            {
                deltaJ[j] = cbInit[j] - zj[j];
            }
            Console.WriteLine("dj");
            Console.WriteLine(JsonSerializer.Serialize(deltaJ));
            return deltaJ;
        }

        private double[] CalculateZj(double[][] table, double[] cb)
        {
            var zj = new double[variabile + restrictii];
            for (int j = 0; j < variabile + restrictii; j++)
            {
                for (int i = 0; i < restrictii; i++)
                {
                    zj[j] += cb[i] * table[i][j];
                }
            }
            return zj;
        }

        private string[] GenerateTableHeaders()
        {
            var headers = new List<string>();
            headers.Add("Baza");
            headers.Add("CB");
            headers.Add("XB");
            for (int i = 0; i < variabile; i++)
            {
                headers.Add($"X{i + 1}");
            }
            for (int i = 0; i < restrictii; i++)
            {
                headers.Add($"S{i + 1}");
            }

            return headers.ToArray();
        }

        private string[][] GenerateTableRows(double[][] table, double[] Baza, double[] CB, double[] XB)
        {

            var rows = new List<string[]>();
            for (int i = 0; i < table.Length; i++)
            {
                var row = new List<string>();
                row.Add(Baza[i] < variabile ? $"X{Baza[i] + 1}" : $"S{i + 1}");
                row.Add(CB[i].ToString());
                row.Add(DoubleToFractionString(XB[i]));
                for (int j = 0; j < table[i].Length; j++)
                {
                    row.Add(DoubleToFractionString(table[i][j]));
                }
                // row.Add(Baza[i].ToString());

                rows.Add(row.ToArray());
            }
            return rows.ToArray();
        }
        public class SimplexStep
        {
            public int StepNumber { get; set; }
            public string[] TableHeaders { get; set; }
            public string[][] TableRows { get; set; }
            public string[] DeltaJ { get; set; }
            public string[] CB { get; set; }
            public string[] ZJ { get; set; }
            public string[] XB { get; set; }
            public string[] Baza { get; set; }
            public string PivotElement { get; set; }
            public string ZK { get; set; }
            public int PivotRow { get; set; } = -1;
            public int PivotColumn { get; set; } = -1;
            public List<string> Observatii { get; set; } = new List<string>();
        }

        private static string DoubleToFractionString(double input)
        {
            const double epsilon = 0.0001;
            int sign = Math.Sign(input);
            input = Math.Abs(input);
            int whole = (int)input;
            input -= whole;
            if (input < epsilon)
            {
                return sign * whole + "";
            }
            else if (1 - epsilon < input)
            {
                return sign * (whole + 1) + "";
            }
            else
            {
                int num = 1;
                int den = 1;
                double error = input;
                while (Math.Abs(num / (double)den - input) > epsilon)
                {
                    if (num / (double)den > input)
                    {
                        den++;
                    }
                    else
                    {
                        num++;
                    }
                }
                int gcd = Gcd(num, den);
                num /= gcd;
                den /= gcd;
                if (whole > 0)
                {
                    num += whole * den;
                }
                return (sign == -1 ? "-" : "") + num + "/" + den;
            }
        }

        private static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
        private string[] DoubleToFractionString(double[] input)
        {
            return input.Select(DoubleToFractionString).ToArray();
        }
    }
}