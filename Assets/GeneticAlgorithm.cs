using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere {
    public float radius;
    public Vector3 position;
    public Sphere(float _radius, Vector3 _position) {
        radius = _radius;
        position = _position;
    }
    public Sphere() {
        radius = 0;
        position = Vector3.zero;
    }
}

public class Cloud {
    public List<Sphere> spheres;
    public float fitness;
    public Cloud (List<Sphere> _sphere, float _fitness) {
        spheres = new List<Sphere>();
        for (int i = 0; i<_sphere.Count; i++) {
            spheres.Add( new Sphere(_sphere[i].radius, _sphere[i].position));
        }
        fitness = _fitness;
    }
    public Cloud() {
        spheres = new List<Sphere>();
    }
}

public class GeneticAlgorithm : MonoBehaviour {
    //Array of clouds [cloud number][ball number][x,y,z,size]
    //float[,,] cloud;
    public GameObject cloudBall;
    [Range(0,1)]
    public float mutationRate;
    public float treshold;
    public float delayTime;

    [Header("Population Size")]
    public int cloudNumber;

    [Header("Number of Chromosomes")]
    public int cloudBallNumber;

    [Header("Random Limit")]
    public Vector3 maximum;
    public Vector3 minimum;
    public float maxRadius;
    public float minRadius;

    List <Cloud> clouds = new List<Cloud>();
    int iteration = 0;

    public void RunGeneticAlgorithm() {
        iteration = 0;
        clouds.Clear();
        StartCoroutine(GeneticAlgorithmProcess());
    }

    void PopulationInitialization() {
        for (int n = 0; n < cloudNumber; n++) {
            List<Sphere> spheres = new List <Sphere>();
            //Debug.Log(spheres.Count);
            for (int m = 0; m < cloudBallNumber; m++) {
                //Debug.Log(m);
                spheres.Add (new Sphere(Random.Range(minRadius, maxRadius), new Vector3(
                Random.Range(minimum.x, maximum.x),
                Random.Range(minimum.y, maximum.y),
                Random.Range(minimum.z, maximum.z)))); 
            }
            Cloud temp = new Cloud(spheres, 100);
            //Debug.Log(temp.spheres.Count);
            clouds.Add(temp);
        }
    }

    void CheckPopulation(Cloud cloud) {
        GameObject cloudMesh = new GameObject("Cloud");
        for (int m = 0; m < cloudBallNumber; m++) {
            GameObject tempCloudBall = Instantiate(cloudBall);
            tempCloudBall.transform.SetParent(cloudMesh.transform);
            tempCloudBall.transform.position = cloud.spheres[m].position;
            tempCloudBall.transform.localScale = Vector3.one * cloud.spheres[m].radius;
        }
    }

    float FindPopulationFitness(int index) {
        float fitness = 0;
        for (int n = 0; n < cloudBallNumber - 1; n++) {
            for (int m = n; m < cloudBallNumber; m++) {
                Vector3 pn = clouds[index].spheres[n].position; ;
                Vector3 pm = clouds[index].spheres[m].position; ;
                fitness += (Mathf.Abs((Vector3.Distance(pn, pm)) - clouds[index].spheres[n].radius - clouds[index].spheres[m].radius));
            }
        }
        return 1.0f/fitness;
    }

    List<Cloud> ParentSelection() {
        //float[] fitnessValues = new float[cloudNumber];
        //Debug.Log(iteration+"Selecting Parent");
        float totalFitnessValues = 0;

        List<Cloud> temp =  new List<Cloud>();

        for (int i = 0; i < cloudNumber; i++) {
            float fitnessValue = FindPopulationFitness(i);
            clouds[i].fitness = fitnessValue;
            //fitnessValues[i] = 1.0f / fitnessValue;
            totalFitnessValues += fitnessValue;
            //Debug.Log(i + " | " + fitnessValue);
        }

        for (int i = 0; i < 2; i++) {
            int tempIndex = 0;
            float randomTreshold = Random.Range(0, totalFitnessValues);
            float tempSum = 0;
            while (tempSum <= randomTreshold && tempIndex<clouds.Count) {
                tempSum += clouds[tempIndex].fitness;
                if (tempIndex < clouds.Count-1) tempIndex++;
            }
            //.Log("TempIndex = " + tempIndex + " Fitness:" + clouds[tempIndex].fitness);
            temp.Add( clouds[tempIndex]);
            //Debug.Log(temp);
        }
        return temp;
    }

    List<Cloud> Crossover() {
        //Debug.Log(iteration + "Crossing Over");
        List<Cloud> children = ParentSelection();
        for (int i = 0; i < cloudBallNumber; i++) {
            if (Random.value > 0.5) {
                Sphere temp = children[0].spheres[i];
                children[0].spheres[i] = children[1].spheres[i];
                children[1].spheres[i] = temp;
            }
        }
        return clouds;
    }

    List<Cloud> Mutation() {
        //Debug.Log(iteration + "Mutating");
        int membersAmount = cloudNumber * cloudBallNumber;
        int mutatedMembersAmount = (int)(membersAmount * mutationRate);
         
        List<Cloud> children = Crossover();
        for (int n = 0; n < mutatedMembersAmount; n++) {
            int randomPos = Random.Range(0, membersAmount);
            for (int i = 0; i < cloudNumber; i++) {
                for (int j = 0; j < cloudBallNumber; j++) {
                    if (i * cloudBallNumber + j > randomPos) break;
                    else if (i * cloudBallNumber + j == randomPos) {
                        clouds[i].spheres[j].radius = Random.Range(minRadius, maxRadius);
                        clouds[i].spheres[j].position = new Vector3(
                            Random.Range(minimum.x, maximum.x),
                            Random.Range(minimum.y, maximum.y),
                            Random.Range(minimum.z, maximum.z));
                        break;
                    }
                }
            }
        }
        return children;
    }

    List<Cloud> DecodeAndFitnessCalculation() {
        //Debug.Log(iteration + "Decoding and Calculating Fitness");
        List<Cloud> children = clouds;
        //for (int i = 0; i < clouds.Count; i++) children.Add(clouds[i]);
        //Debug.Log("PANJANG TOTAL " + children.Count);
            //clouds.Concat(Mutation()).ToArray();
        for (int index = 0; index < children.Count; index++) {
            float fitness = 0;
            for (int n = 0; n < cloudBallNumber - 1; n++) {
                for (int m = n; m < cloudBallNumber; m++) {
                    //Debug.Log(index);
                    Vector3 pn = children[index].spheres[n].position; ;
                    Vector3 pm = children[index].spheres[m].position; ;
                    fitness += 1/((Vector3.Distance(pn, pm)) - children[index].spheres[n].radius - children[index].spheres[m].radius);
                }
            }
            children[index].fitness = fitness;
        }
        return children;
    }

    List<Cloud> SurvivorSelection() {
       // Debug.Log(iteration + "Selecting Survivor");
        List<Cloud> children = DecodeAndFitnessCalculation();
        int lowestFitnessIndex = 0;
        int runnerUpIndex = 0;
        for (int i = 0; i < children.Count; i++) {
            if (children[i].fitness < children[lowestFitnessIndex].fitness) lowestFitnessIndex = i;
            else if (children[i].fitness < children[runnerUpIndex].fitness) runnerUpIndex = i;
        } 
        if (runnerUpIndex == 0) {
            runnerUpIndex = 1;
            for (int i = 1; i < children.Count; i++) {
                if (children[i].fitness < children[runnerUpIndex].fitness) runnerUpIndex = i;
            }
        }
        if (lowestFitnessIndex < runnerUpIndex) {
            children.RemoveAt(lowestFitnessIndex);
            children.RemoveAt(runnerUpIndex - 1);
        }
        else {
            //Debug.Log(lowestFitnessIndex + " | " + runnerUpIndex);
            children.RemoveAt(runnerUpIndex);
            children.RemoveAt(lowestFitnessIndex-1);
        }
        return children;
    }

    Cloud FindBest() {
        //Debug.Log(iteration + "Finding Best");
        clouds = Mutation();
        Cloud best = clouds[0];
        for (int i = 0; i<clouds.Count; i++) {
            float fitnessValue = FindPopulationFitness(i);
            clouds[i].fitness = fitnessValue;
            if (clouds[i].fitness > best.fitness) best = clouds[i];
        }
        //if (1 / best.fitness > 1) return FindBest();
        return best;
    }

    IEnumerator GeneticAlgorithmProcess() {
        PopulationInitialization();
        float fitness = 0;
        Cloud bestCloud = new Cloud();
        do {
            //foreach (MeshRenderer g in FindObjectsOfType<MeshRenderer>()) Destroy(g);
            iteration++;

            Destroy(GameObject.Find("Cloud"));
            bestCloud = FindBest();
            CheckPopulation(bestCloud);
            fitness = 1 / bestCloud.fitness;
            Debug.Log("Generasi: " + iteration+" Fitness: "+fitness);
            //yield return null;
            yield return new WaitForSeconds(delayTime);
        } while (fitness>treshold);
        //CheckPopulation(Mutation()[0]);
    }

}
