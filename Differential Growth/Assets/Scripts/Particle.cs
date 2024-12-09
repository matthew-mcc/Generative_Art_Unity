using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;



public class Particle{
    
    public Vector2 position;
    public Vector2 velocity;
    SpriteRenderer spriteRenderer;
    // float repulusionDistance = 2.5f;
    // float k = 0.0001f;
    float repulsionDistance;
    float k;
    float insertDistance;
    float velocityDamping;

    public Particle(float x, float y, SimSettings settings){
        position = new Vector2(x, y);
        velocity = new Vector2(0, 0);

        this.repulsionDistance = settings.insertDistance * 5;
        this.k = settings.k;
        this.insertDistance = settings.insertDistance;
        this.velocityDamping = settings.velocityDamping;

        

    }
    public void update(List<Particle> c, int n){
        Vector2 diff = new Vector2(0, 0);
        Vector2 forces = new Vector2(0, 0);
        float distance;

        // Repulsive forces
        foreach(Particle p in c){
            if(p != this){
                diff = p.position - position;
                distance = diff.magnitude;

                if (distance < repulsionDistance){
                    diff.Normalize();
                    diff = diff * -1 / (distance * distance);
                    // forces.
                    forces += diff;
                }
            }
        }

        // For mass 1, this is unecessary
        // Vector2 acceleration = new Vector2(0, 0);
        // acceleration = forces;
        // acceleration = acceleration / 1;
        // velocity += acceleration;

        // Atractive force "left" neighbor
        int neighbor = (n + 1) % c.Count;
        Particle temp = c[neighbor];
        diff = temp.position - position;
        distance = diff.magnitude;
        diff.Normalize();
        diff = diff * (1 / (distance*distance));
        
        if(distance < insertDistance / 2){    
            diff *= -1;
        }
        forces += diff;

        
        // Attractive force "right" neighbor
        neighbor = ((n - 1) + c.Count) % c.Count;
        temp = c[neighbor];
        diff = temp.position - position;
        distance = diff.magnitude;
        diff.Normalize();
        diff = diff * (1 / (distance*distance));
        if(distance < insertDistance / 2){     
            diff *= -1;
        }
        forces += diff;
        

        forces = forces * k; // similar to spring constant
        velocity += forces;
    }

    public void updatePosition(){
        position += velocity;
        
        velocity = velocity * velocityDamping;
    }

    public void display(){
        
        spriteRenderer.transform.position = new Vector3(position.x, position.y, 0);
    }
}