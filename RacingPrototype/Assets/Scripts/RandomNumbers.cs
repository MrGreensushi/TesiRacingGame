using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

public class RandomNumbers
{
    // Start is called before the first frame update
    public static HashSet<int> Get (int numeroDaGenerare,int minimo,int massimo)
    {
        // Debug.LogError("N: "+numeroDaGenerare+" min: "+minimo+" max: "+massimo);
        // Verifica che il range sia sufficiente per il numero richiesto
        Assert.IsFalse(numeroDaGenerare > (massimo - minimo));
        

        // Crea l'istanza di Random
        var random = new Random();

        // Lista per salvare i numeri casuali
        HashSet<int> numeriCasuali = new HashSet<int>();

        // Genera i numeri casuali unici
        while (numeriCasuali.Count < numeroDaGenerare)
        {
            int numeroCasuale = random.Next(minimo, massimo); // massimo è esclusivo

            // Il Set aggiunge solo se il numero non esiste già
            numeriCasuali.Add(numeroCasuale);
        }
        
        return numeriCasuali;
    }
    

}
