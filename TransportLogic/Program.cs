using TransportLogic.Models;
using TransportLogic.Structures;

//------------Исходные данные------------//
List<List<Cell>> matrix = new List<List<Cell>>
{
    new List<Cell>
    {
        new Cell {Cost = 6, Supplie = 0},
        new Cell {Cost = 2, Supplie = 0},
        new Cell {Cost = 10, Supplie = 0},
        new Cell {Cost = 12, Supplie = 0}
    },
    new List<Cell>
    {
        new Cell {Cost = 8, Supplie = 0},
        new Cell {Cost = 14, Supplie = 0},
        new Cell {Cost = 1, Supplie = 0},
        new Cell {Cost = 13, Supplie = 0}
    },
    new List<Cell>
    {
        new Cell {Cost = 8, Supplie = 0},
        new Cell {Cost = 5, Supplie = 0},
        new Cell {Cost = 2, Supplie = 0},
        new Cell {Cost = 7, Supplie = 0}
    }
};
List<List<TempPeak>> helpMatrix = new List<List<TempPeak>>
{
    new List<TempPeak>
    {
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false}
    },
    new List<TempPeak>
    {
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false}
    },
    new List<TempPeak>
    {
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false},
        new TempPeak {Value = 0, PeakOnWay = false}
    }
};

List<int> providers = new List<int> { 30, 20, 50 };
List<int> consumers = new List<int> { 25, 25, 40, 10 };


//------------Вспомогательные массивы------------//
List<ABValue> alphas = new List<ABValue>();
List<ABValue> betas= new List<ABValue>();
List<PBCellLoc> potentials = new List<PBCellLoc>();
List<PBCellLoc> bases = new List<PBCellLoc>();
List<PBCellLoc> resBases = new List<PBCellLoc>();
List<Peak> peaks = new List<Peak>();

bool notPeaksInColumn = true;
bool notPeaksInRow = true;
bool loopBuilded = false;
bool wrongWay;

//------------Работа------------//
PotentialMethod();


//------------Проверки и заполнение массивов перед решением------------//
#region DataValidAndFillingArrays
void AddBasis()
{
    for (int i = 0; i < providers.Count; i++)
    {
        alphas.Add(new ABValue { Value = 0, Changed = false});
    }
    for (int i = 0; i < consumers.Count; i++)
    {
        betas.Add(new ABValue { Value = 0, Changed = false });
    }
}

void CheckBalance(ref bool balance, ref int provSum, ref int consSum)
{
    for (int i = 0; i < providers.Count; i++)
    {
        provSum += providers[i];
    }
    for (int i = 0; i < consumers.Count; i++)
    {
        consSum += consumers[i];
    }
    if (provSum == consSum)
        balance = true;
}
#endregion


//------------Заполнение методом северо-западного угла------------//
#region SolvingMethods
void NorthwestCornerMethodFilling()
{
    int provSum = 0;
    int consSum = 0;
    bool balance = false;
    int lastSupplie = 0;
    CheckBalance(ref balance, ref provSum, ref consSum);
    AddBasis();
    for (int i = 0; i < matrix.Count; i++)
    {
        for (int j = 0; j < matrix[i].Count; j++)
        {
            if (providers[i] != 0)
            {
                if (providers[i] <= consumers[j])
                {
                    matrix[i][j].Supplie = providers[i];
                    consumers[j] -= providers[i];
                    providers[i] -= providers[i];
                }              
            }
            if (consumers[j] != 0)
            {
                if (providers[i] >= consumers[j])
                {
                    matrix[i][j].Supplie = consumers[j];
                    providers[i] -= consumers[j];
                    consumers[i] -= consumers[i];
                }
            }
        }
    }
    SetLocationOfPotentials();
    SetLocationOfBases();

    if (balance == false)
    {
        if(provSum > consSum)
        {
            for(int i = 0; i < providers.Count; i++)
            {
                lastSupplie = providers[i];
            }
            providers.Add(lastSupplie);
            matrix.Add(new List<Cell>
            {
                new Cell{Cost = 0, Supplie = 0},
                new Cell{Cost = 0, Supplie = 0},
                new Cell{Cost = 0, Supplie = 0},
                new Cell{Cost = 0, Supplie = 0},
            });
            for (int i = 0; i < providers.Count; i++)
            {
                if(i == providers.Count - 1)
                {
                    matrix[i][matrix[i].Count].Supplie = lastSupplie;
                }
            }
        }
        else if(provSum < consSum)
        {
            for (int i = 0; i < consumers.Count; i++)
            {
                lastSupplie = consumers[i];
            }
            providers.Add(lastSupplie);
            matrix.Add(new List<Cell>
            {
                new Cell{Cost = 0, Supplie = 0},
                new Cell{Cost = 0, Supplie = 0},
                new Cell{Cost = 0, Supplie = 0},
                new Cell{Cost = 0, Supplie = 0},
            });
            for (int i = 0; i < consumers.Count; i++)
            {
                if (i == consumers.Count - 1)
                {
                    matrix[i][matrix[i].Count - 1].Supplie = lastSupplie;
                }
            }
        }
    }
    
}

void SetLocationOfPotentials()
{
    potentials.Clear();
    for (int i = 0; i < matrix.Count; i++)
    {
        for (int j = 0; j < matrix[i].Count; j++)
        {
            if(matrix[i][j].Supplie > 0)
                potentials.Add(new PBCellLoc { i = i, j = j });
        }
    }
}

void SetLocationOfBases()
{
    bases.Clear();
    for (int i = 0; i < matrix.Count; i++)
    {
        for (int j = 0; j < matrix[i].Count; j++)
        {
            if (matrix[i][j].Supplie == 0)
                bases.Add(new PBCellLoc { i = i, j = j });
        }
    }
}

void EquationBuilder()
{
    for (int i = 0; i < potentials.Count; i++)
    {
        if (i == 0)
        {
            alphas[potentials[i].i].Value = matrix[potentials[i].i][potentials[i].j].Cost;
            betas[potentials[i].j].Value = matrix[potentials[i].i][potentials[i].j].Cost - alphas[potentials[i].i].Value;
            alphas[potentials[i].i].Changed = true;
            betas[potentials[i].j].Changed = true;
        }
        else
        {
            if(betas[potentials[i].j].Changed == false)
            {
                betas[potentials[i].j].Value = matrix[potentials[i].i][potentials[i].j].Cost - alphas[potentials[i].i].Value;
                betas[potentials[i].j].Changed = true;
            }
            else if(alphas[potentials[i].i].Changed == false)
            {
                alphas[potentials[i].i].Value = matrix[potentials[i].i][potentials[i].j].Cost - betas[potentials[i].j].Value;
                alphas[potentials[i].i].Changed = true;
            }
        }
        Console.Write(" " + $"A{potentials[i].i}" + "|" + alphas[potentials[i].i].Value + " ");
        Console.Write(" " + $"B{potentials[i].j}" + "|" + betas[potentials[i].j].Value + " ");
    }
}

void AddResolvingBases()
{
    for (int i = 0; i < bases.Count; i++)
    {
        if (alphas[bases[i].i].Value + betas[bases[i].j].Value <= matrix[bases[i].i][bases[i].j].Cost)
            continue;
        else
            resBases.Add(new PBCellLoc { i = bases[i].i, j = bases[i].j });
    }
}

void SolvingLoopBuilder(int iterator, int order, int bi, int bj)
{
    if (wrongWay == false && loopBuilded == false)
    {
        if (iterator < (matrix.Count + matrix[0].Count) - 1)
        {
            iterator++;
            if (order == 0)
            {
                int ii = bi;
                for (int i = 0; i < matrix.Count; i++)
                {
                    if (matrix[i][bj].Supplie > 0 && i != bi && helpMatrix[i][bj].PeakOnWay == false)
                    {
                        ii = i;
                        helpMatrix[i][bj].Value = 1;
                        helpMatrix[i][bj].PeakOnWay = true;
                        peaks.Add(new Peak { i = i, j = bj });
                        notPeaksInColumn = false;
                        break;
                    }
                    if(iterator >= 3 && peaks[0].i == i && peaks[0].j == bj )
                    {
                        loopBuilded = true;
                        break;
                    }
                    else
                    {
                        notPeaksInColumn = true;
                    }
                }
                if (notPeaksInColumn == true && notPeaksInRow == true)
                {
                    loopBuilded = false;
                    wrongWay = true;
                    for (int i = 0; i < peaks.Count; i++)
                    {
                        helpMatrix[peaks[i].i][peaks[i].j].Value = 0;
                    }
                    SolvingLoopBuilder(iterator, 0, peaks[0].i, peaks[0].j);
                }
                else
                    SolvingLoopBuilder(iterator, 1, ii, bj);
            }
            else if (order == 1)
            {
                int jj = bj;
                for (int j = 0; j < matrix[0].Count; j++)
                {
                    if (matrix[bi][j].Supplie > 0 && j != bj && helpMatrix[bi][j].PeakOnWay == false)
                    {
                        jj = j;
                        helpMatrix[bi][j].Value = 1;
                        helpMatrix[bi][j].PeakOnWay = true;
                        peaks.Add(new Peak {i = bi, j = j });
                        notPeaksInRow = false;
                        break;
                    }
                    if (iterator >= 3 && peaks[0].i == bi && peaks[0].j == j)
                    {
                        loopBuilded = true;
                        break;
                    }
                    else
                    {
                        notPeaksInRow = true;
                    }
                }
                if (notPeaksInColumn == true && notPeaksInRow == true)
                {
                    loopBuilded = false;
                    wrongWay = true;
                    for (int i = 0; i < peaks.Count; i++)
                    {
                        helpMatrix[peaks[i].i][peaks[i].j].Value = 0;
                    }
                    SolvingLoopBuilder(iterator, 0, peaks[0].i, peaks[0].j);
                }
                else
                    SolvingLoopBuilder(iterator, 0, bi, jj);
            }
        }
    }
    else if(loopBuilded != true)
        peaks.Clear();
}

void DistributeSigns(ref int[] ints)
{
    int order = 0;
    for (int i = 0; i < peaks.Count; i++)
    {
        if (peaks[i].Flag != true)
        {
            ints[i - 1] = matrix[peaks[i].i][peaks[i].j].Supplie;
        }
        switch (order)
        {
            case 0:
                peaks[i].Sign = '+';
                order = 1;
                break;
            case 1:
                peaks[i].Sign = '-';
                order = 0;
                break;
        }
        Console.Write(peaks[i].Sign + " ");
    }
}

void Permutation(ref int[] ints)
{
    int minValue = ints.Min();
    for (int i = 0; i < peaks.Count; i++)
    {
        if (peaks[i].Sign == '+')
        {
            matrix[peaks[i].i][peaks[i].j].Supplie += minValue;
        }
        else if(peaks[i].Sign == '-')
        {
            matrix[peaks[i].i][peaks[i].j].Supplie -= minValue;
        }
    }
}
#endregion

//------------Решение методом потенциалов------------//
#region MainMethod
void PotentialMethod()
{
    int iterator = 0;
    CostMatrixOutput();
    EnterX2();
    NorthwestCornerMethodFilling();
    EnterX2();
    SupplieMatrixOutput();
    EnterX2();
    EquationBuilder();
    AddResolvingBases();
    EnterX2();
    for (int i = 0; i < resBases.Count; i++)
    {
        HelpMatrixOutput();
        EnterX2();
        wrongWay = false;
        peaks.Add(new Peak { i = resBases[i].i, j = resBases[i].j, Sign = '+', Flag = true});
        SolvingLoopBuilder(iterator, 0, resBases[i].i, resBases[i].j);
        if (loopBuilded == true)
        {
            int[] ints = new int[peaks.Count - 1];
            DistributeSigns(ref ints);
            Permutation(ref ints);
            SupplieMatrixOutput();
            loopBuilded = false;
            peaks.Clear();
        }
    }
}
#endregion

//------------Методы вывода на экран------------//
#region Output
void EnterX2()
{
    Console.WriteLine();
    Console.WriteLine();
}

void CostMatrixOutput()
{
    for (int i = 0; i < matrix.Count; i++)
    {
        Console.WriteLine();
        for (int j = 0; j < matrix[i].Count; j++)
        {
            Thread.Sleep(50);
            Console.Write($"{matrix[i][j].Cost} ");
        }
    }
}

void HelpMatrixOutput()
{
    for (int i = 0; i < matrix.Count; i++)
    {
        Console.WriteLine();
        for (int j = 0; j < matrix[i].Count; j++)
        {
            Console.Write($"{helpMatrix[i][j].Value} ");
        }
    }
}

void SupplieMatrixOutput()
    {
        for (int i = 0; i < matrix.Count; i++)
        {
            Console.WriteLine();
            for (int j = 0; j < matrix[i].Count; j++)
            {
                Thread.Sleep(50);
                Console.Write($"{matrix[i][j].Supplie} ");
            }
        }
    }
#endregion