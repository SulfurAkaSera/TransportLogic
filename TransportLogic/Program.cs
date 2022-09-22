using TransportLogic.Models;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using TransportLogic.Structures;

//------------Исходные данные------------//
List<List<Cell>> matrix = new List<List<Cell>>
{
    new List<Cell>
    {
        new Cell {Cost = 3, Supplie = 0},
        new Cell {Cost = 5, Supplie = 0},
        new Cell {Cost = 10, Supplie = 0},
        new Cell {Cost = 11, Supplie = 0}
    },
    new List<Cell>
    {
        new Cell {Cost = 8, Supplie = 0},
        new Cell {Cost = 4, Supplie = 0},
        new Cell {Cost = 7, Supplie = 0},
        new Cell {Cost = 3, Supplie = 0}
    },
    new List<Cell>
    {
        new Cell {Cost = 8, Supplie = 0},
        new Cell {Cost = 9, Supplie = 0},
        new Cell {Cost = 2, Supplie = 0},
        new Cell {Cost = 1, Supplie = 0}
    }
};

List<int> providers = new List<int> { 30, 20, 40 };
List<int> consumers = new List<int> { 20, 20, 40, 10 };


//------------Вспомогательные массивы------------//
List<ABValue> alphas = new List<ABValue>();
List<ABValue> betas= new List<ABValue>();
List<PBCellLoc> potentials = new List<PBCellLoc>();
List<PBCellLoc> bases = new List<PBCellLoc>();
List<PBCellLoc> resBases = new List<PBCellLoc>();
List<Peak> peaks = new List<Peak>();

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
    Console.WriteLine();
    for (int i = 0; i < providers.Count; i++)
    {
        provSum += providers[i];
    }
    Console.WriteLine(provSum);
    for (int i = 0; i < consumers.Count; i++)
    {
        consSum += consumers[i];
    }
    Console.WriteLine(consSum);
    if (provSum == consSum)
        balance = true;
    Console.WriteLine(balance.ToString());
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
    int j = 0;
    for (int i = 0; i < bases.Count; i++)
    {
        if (alphas[bases[i].i].Value + betas[bases[i].j].Value <= matrix[bases[i].i][bases[i].j].Cost)
            continue;
        else
            resBases.Add(new PBCellLoc { i = bases[i].i, j = bases[i].j });
    }
}

void SolvingLoopBuilder( int bi, int bj)
{

}
#endregion

//------------Решение методом потенциалов------------//
#region MainMethod
void PotentialMethod()
{
    CostMatrixOutput();
    EnterX2();
    NorthwestCornerMethodFilling();
    EnterX2();
    SupplieMatrixOutput();
    EnterX2();
    EquationBuilder();
    AddResolvingBases();
    for (int i = 0; i < resBases.Count; i++)
    {
        if(i == 0)
            peaks.Add(new Peak { I = resBases[i].i, J = resBases[i].j, CorrectCell = true, Flag = true, Sign = '+' });
        SolvingLoopBuilder(resBases[i].i, resBases[i].j);
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