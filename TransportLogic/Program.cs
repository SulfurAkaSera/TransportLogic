using TransportLogic.Models;
using TransportLogic.Structures;

/*
 Итак, значится, можешь выкинуть все в отдельный клас и сделать так, чтобы пользователь
 мог кидать исходные данные через конструктор например.
 Все переменные, массивы и методы, за исключением исходных данных, тесно связаны и должны быть в
 одном классе приватными, иначе все поломается.
 Но, во всяком случае, ты можешь разобраться в алгоритме сам и сделать все так как считаешь нужным.
 Только вот если ты вообще все перелопатил - ко мне не обращайся.
*/


//------------Исходные данные------------//
#region Initial data
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
List<int> providers = new() { 30, 20, 50 };
List<int> consumers = new() { 25, 25, 40, 10 };
#endregion

//------------Вспомогательные массивы и переменные------------//
#region Arrays and variables
List<List<TempPeak>> helpMatrix = new List<List<TempPeak>>();
List<ABValue> alphas = new();
List<ABValue> betas= new();
List<PBCellLoc> potentials = new();
List<PBCellLoc> bases = new();
List<PBCellLoc> resBases = new();
List<LPeak> lPeaks = new();
List<List<Peak>> way = new();

bool notPeaksInColumn = false;
bool notPeaksInRow = false;
bool loopBuilded = false;
bool wrongBasis = false;
bool wrongWay;
bool wrongAllBases;
#endregion

//------------Вызов главного метода------------//
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

void HelpMatrixDefault()
{
    helpMatrix.Clear();
    for (int i = 0; i < matrix.Count; i++)
    {
        helpMatrix.Add(new List<TempPeak>());
        for(int j = 0; j < matrix[i].Count; j++)
        {
            helpMatrix[i].Add(new TempPeak { Value = 0, PeakOnWay = false, WrongPeak = false});
        }
    }
}
#endregion

//------------Решение задачи------------//
#region SolvingMethods
void PreparingForTheLoop()
{
    SetLocationOfPotentials();
    SetLocationOfBases();
    EquationBuilder();
    AddResolvingBases();
}

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
    resBases.Clear();
    for (int i = 0; i < bases.Count; i++)
    {
        if (alphas[bases[i].i].Value + betas[bases[i].j].Value <= matrix[bases[i].i][bases[i].j].Cost)
            continue;
        else
            resBases.Add(new PBCellLoc { i = bases[i].i, j = bases[i].j });
    }
}

void SolvingLoopBuilder(int bi, int bj)
{
    //Ну, довай, удачи разобраться)
    int pi = bi;
    int pj = bj;
    var column = new List<Peak>();
    var row = new List<Peak>();
    int iterator = 0;
    int order = 0;
    Way();
    int Way()
    {
        if (loopBuilded == false && wrongBasis != true)
        {
            if (order == 0)
            {
                order = 1;

                if (wrongWay != true)
                    way.Add(new List<Peak>());
                else
                {
                    way.RemoveAt(iterator);
                    if (iterator > 0)
                        iterator -= 1;
                    if (way.Count == 0)
                    {
                        loopBuilded = false;
                        wrongBasis = true;
                    }
                    else
                        pj = way[iterator][0].j;
                }

                column.Clear();
                for (int i = 0; i < helpMatrix.Count; i++)
                {
                    if (matrix[i][pj].Supplie > 0 && helpMatrix[i][pj].PeakOnWay != true && helpMatrix[i][pj].WrongPeak != true)
                    {
                        column.Add(new Peak { i = i, j = pj });
                    }
                    if (iterator >= 3)
                    {
                        for (int k = 0; k < lPeaks.Count; k++)
                        {
                            if (lPeaks[k].i == i && lPeaks[k].j == pj && lPeaks[k].Flag == true)
                            {
                                loopBuilded = true;
                                return 0;
                            }
                        }
                    }
                }

                if (column.Count > 0)
                    notPeaksInColumn = false;
                else
                    notPeaksInColumn = true;

                if (notPeaksInColumn != true)
                {
                    way[iterator].AddRange(column);
                    try
                    {
                        for (int k = 0; k < way[iterator].Count; k++)
                        {
                            if (helpMatrix[way[iterator][k].i][way[iterator][k].j].WrongPeak != true)
                            {
                                pi = way[iterator][k].i;
                                pj = way[iterator][k].j;
                                helpMatrix[pi][pj].PeakOnWay = true;
                                lPeaks.Add(new LPeak { i = pi, j = pj, Flag = false, Sign = '\0' });
                                iterator++;
                                Way();
                            }
                        }
                    }
                    catch
                    {
                        return 0;
                    }
                }
                else
                {
                    helpMatrix[pi][pj].WrongPeak = true;
                    wrongWay = true;
                    Way();
                }

            }
            else if (order == 1)
            {
                order = 0;

                if (wrongWay != true)
                    way.Add(new List<Peak>());
                else
                {
                    way.RemoveAt(iterator);
                    if (iterator > 0)
                        iterator -= 1;
                    if (way.Count == 0)
                    {
                        loopBuilded = false;
                        wrongBasis = true;
                    }
                    else
                        pi = way[iterator][0].i;
                }


                row.Clear();
                for (int j = 0; j < helpMatrix[0].Count; j++)
                {
                    if (matrix[pi][j].Supplie > 0 && helpMatrix[pi][j].PeakOnWay != true && helpMatrix[pi][j].WrongPeak != true)
                    {
                        row.Add(new Peak { i = pi, j = j });
                    }
                    if (iterator >= 3)
                    {
                        for (int k = 0; k < lPeaks.Count; k++)
                        {
                            if (lPeaks[k].i == pi && lPeaks[k].j == j && lPeaks[k].Flag == true)
                            {
                                loopBuilded = true;
                                return 0;
                            }
                        }
                    }
                }

                if (row.Count > 0)
                    notPeaksInRow = false;
                else
                    notPeaksInRow = true;

                if (notPeaksInRow != true)
                {
                    way[iterator].AddRange(row);
                    try
                    {
                        for (int k = 0; k < way[iterator].Count; k++)
                        {
                            if (helpMatrix[way[iterator][k].i][way[iterator][k].j].WrongPeak != true)
                            {
                                pi = way[iterator][k].i;
                                pj = way[iterator][k].j;
                                helpMatrix[pi][pj].PeakOnWay = true;
                                lPeaks.Add(new LPeak { i = pi, j = pj, Flag = false, Sign = '\0' });
                                iterator++;
                                Way();
                            }
                        }
                    }
                    catch
                    {
                        return 0;
                    }
                }
                else
                {
                    helpMatrix[pi][pj].WrongPeak = true;
                    wrongWay = true;
                    Way();
                }
            }
        }
        else
        {
            lPeaks.Clear();
            HelpMatrixDefault();
        }
        return 0;
    }
}

void DistributeSigns(ref int[] ints)
{
    int order = 0;
    for (int i = 0; i < lPeaks.Count; i++)
    {
        if (lPeaks[i].Flag != true)
        {
            ints[i - 1] = matrix[lPeaks[i].i][lPeaks[i].j].Supplie;
        }
        switch (order)
        {
            case 0:
                lPeaks[i].Sign = '+';
                order = 1;
                break;
            case 1:
                lPeaks[i].Sign = '-';
                order = 0;
                break;
        }
        Console.Write(lPeaks[i].Sign + " ");
    }
}

void Permutation(ref int[] ints)
{
    int minValue = ints.Min();
    for (int i = 0; i < lPeaks.Count; i++)
    {
        if (lPeaks[i].Sign == '+')
        {
            matrix[lPeaks[i].i][lPeaks[i].j].Supplie += minValue;
        }
        else if(lPeaks[i].Sign == '-')
        {
            matrix[lPeaks[i].i][lPeaks[i].j].Supplie -= minValue;
        }
    }
}

void SupplieSum()
{
    int sum = 0;
    for (int i = 0; i < potentials.Count; i++)
    {
        sum += matrix[potentials[i].i][potentials[i].j].Cost * matrix[potentials[i].i][potentials[i].j].Supplie;
    }
    Console.WriteLine(sum);
}
#endregion

//------------Главный метод------------//
#region MainMethod
void PotentialMethod()
{
    CostMatrixOutput();
    EnterX2();
    NorthwestCornerMethodFilling();
    EnterX2();
    SupplieMatrixOutput();
    EnterX2();
    PreparingForTheLoop();
    EnterX2();
    SupplieSum();
    HelpMatrixDefault();
    BuildingLoop();
    EnterX2();
    SupplieSum();
    void BuildingLoop()
    {
        for (int i = 0; i < resBases.Count; i++)
        {
            wrongAllBases = true;
            helpMatrix[resBases[i].i][resBases[i].j].PeakOnWay = true;
            EnterX2();
            wrongWay = false;
            wrongBasis = false;
            lPeaks.Add(new LPeak { i = resBases[i].i, j = resBases[i].j, Sign = '+', Flag = true });
            SolvingLoopBuilder(resBases[i].i, resBases[i].j);
            if (loopBuilded == true)
            {
                int[] ints = new int[lPeaks.Count - 1];
                DistributeSigns(ref ints);
                Permutation(ref ints);
                SupplieMatrixOutput();
                SetLocationOfPotentials();
                EnterX2();
                loopBuilded = false;
                HelpMatrixDefault();
                lPeaks.Clear();
                way.Clear();
            }
            else
            {
                way.Clear();
                if (i == resBases.Count - 1)
                {
                    PreparingForTheLoop();
                    if(wrongAllBases != true)
                        BuildingLoop();
                }

            }
        }
    }
    
}
#endregion

//------------Методы вывода на экран (Это нужно было мне для проверки. Можно удалить)------------//
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
