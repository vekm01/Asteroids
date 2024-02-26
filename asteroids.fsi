module Asteroids

open System

type Pos = (float*float) // Position vector
type Dir = (float*float) // Direction vector

val deltaT : float

val size : int

val mutable gameTime: float

/// <summary> Generates a list of unique pairs from a input list</summary>
/// <param name="lst"> The input list </param>
/// <returns> A list of tuples</returns>
val uniquePairs: list<'a> -> list<('a*'a)>

/// <summary> The functions calculates the magnitude of a vector </summary>
/// <param name="v"> A vector </param>
/// <returns> The magnitude of the vector </returns>
val magnitude: (float*float) -> float 

/// <summary> The function calculates a direction vector's angle to the x-axis </summary>
/// <param name="v"> A vector </param>
/// <returns> An angle in radians </returns>
val vecToAngle: (float*float) -> float

/// <summary> The function produces random float in the interval [min; max) </summary>
/// <param name="min"> A float </param>
/// <param name="max"> A float </param>
/// <returns> A random float </returns>
val randomFloat : float -> float -> float

/// <summary> The functions produces a direction vector corresponding to an angle </summary>
/// <param name="angle"> A vector as a float </param>
/// <returns> A tuple as floats </returns>
val angleToDirVec : float -> (float*float)

/// <summary> Produces tuple of randomly chosen floats in the interval [min; max) </summary>
/// <param name="min"> A float value for the upper bound </param>
/// <param name="max"> A float value for the lower bound </param>
/// <returns> Tuple of floats </returns>
val randomFloatPos : float -> float -> (float*float)

/// <summary> Rotates a vector </summary>
/// <param name="v"> Vector to be rotated </param>
/// <param name="angle"> Angle in radians </param>
/// <returns> The updated direction of the vecto</returns>
val rotate: Dir -> float -> (float*float)

/// <summary> The function computes the distance between two points </summary>
/// <param name="p1"> A point as a tuple of floats </param>
/// <param name="p2"> A point as a tuple of floats </param>
/// <returns> The distance as a float </returns>
val distance : Pos -> Pos -> float


/// <summary> Creates list of n random points that are further than a certain distance from each other </summary>
/// <param name="dist"> A float value specifying distance requirement </param>
/// <param name="n"> An integer value specifying number of points </param>
/// <param name="min"> A float specifying lower bound for coordinates </param>
/// <param name="max"> A float specifying upper bound for coordinates </param>
/// <returns> A list of coordinates </returns>
val randomPosList : float -> int -> float -> float -> list<(float*float)>


/// <summary> Generates a random unit direction vector with an angle between 0 and two pi)</summary>
/// <returns> A random unit direction vector </returns>
val randomDirVec : unit -> (float*float)

type GameObject =
    new: float*float*Pos*Dir*float -> GameObject
    member TimeCreated: float

    /// <summary> Gets or sets the hit status of the GameObject </summary>
    /// <param name="newHit"> The new hit status of the GameObject </param>
    /// <returns> The current hit status of the gameObject</returns>
    member Hit: bool with get, set
    
    /// <summary> Gets or sets the speed of the GameObject </summary>
    /// <param name="newSpeed"> The new speed of the GameObject </param>
    /// <returns> The current speed of the gameObject</returns>
    member Speed: float with get, set
    
    /// <summary> Gets or sets the max speed of the GameObject </summary>
    /// <param name="newMaxSpeed"> The new max speed of the GameObject </param>
    /// <returns> The current max speed of the gameObject</returns>
    member MaxSpeed: float with get, set
    
    /// <summary> Gets or sets the position of the GameObject </summary>
    /// <param name="newPos"> The new position of the GameObject </param>
    /// <returns> The current position of the gameObject</returns>
    member Position: (float*float) with get, set
    
    /// <summary> Gets or sets the direction of the GameObject </summary>
    /// <param name="newDir"> The new direction of the GameObject </param>
    /// <returns> The current direction of the gameObject</returns>
    member Dir: (float*float) with get, set

    member Radius: float
    
    /// <summary> Calculates the new position of the object </summary>
    /// <returns> Unit </returns>
    member NewPosition: unit -> unit
    
    /// <summary> Writes a GameObject as its position and the time when it was created </summary>
    /// <returns> A string </returns>
    override ToString: unit -> string

type Bullet =
    new: float*float*Pos*Dir*float -> Bullet
    inherit GameObject

type Spaceship =
    new: float*float*float*Pos*Dir*float -> Spaceship
    inherit GameObject
    
    /// <summary> Gets or sets the list of bullets </summary>
    /// <param name="newBuls"> The new list of bullets </param>
    /// <returns> The current list of bullets </returns>
    member Bullets : list<Bullet> with get, set

    member Acceleration : float

    /// <summary> Computes a new speed for the spaceship and accelerates its speed </summary>
    /// <returns> Unit </returns>
    member Accelerate : unit -> unit

    /// <summary> Shoots a bullet </summary>
    /// <returns> Unit </returns>
    member Shoot : unit -> unit

    /// <summary> Rotates the spaceship </summary>
    /// <param name="angle"> An angle in radians </param>
    /// <returns> Unit </returns>
    member Turn : float -> unit

type Asteroid =
    new: float*float*Pos*Dir*float -> Asteroid
    inherit GameObject
    
type Game = 
    new: int -> Game
    
    /// <summary> Gets or sets the spaceship in Game </summary>
    /// <param name="newSpaceship"> The new spaceship </param>
    /// <returns> The current spaceship </returns>
    member Spaceship : Spaceship with get, set

    /// <summary> Gets or sets the asteroids in Game </summary>
    /// <param name="newAsteroids"> The new asteroids in Game </param>
    /// <returns> The current asteroids in Game </returns>
    member Asteroids : list<Asteroid> with get, set
    
    /// <summary> Calculates the distance between two game objects </summary>
    /// <param name="obj1"> First game object </param>
    /// <param name="obj2"> Second game object </param>
    /// <returns> The distance as a float </returns>
    member ObjDist :  GameObject -> GameObject -> float

    /// <summary> Checks for and handles collisions </summary
    /// <returns> Unit </returns>  
    member Collisions :  unit -> unit

    /// <summary> Checks if there are any asteroids in the list </summary>
    /// <returns> true if there are no asteroids in the list, otherwise false </returns>
    member GameWon : unit -> bool

    /// <summary> Runs the game for one game tick (deltaT) </summary>
    /// <returns> Unit </returns>
    member Update : unit -> unit
    
type Assert =
    new: unit -> Assert
    
    /// <summary> Enables testing of specific properties using given comparison operator. </summary>
    /// <param name="str"> String displaying desired property. </param>
    /// <param name="a"> Test value. </param>
    /// <param name="b"> Desired outcome. </param>
    /// <param name="f"> Comparison operator with boolean return value. </param>
    /// <returns> Unit. Prints boolean value to screen along with given string. </returns>
    static member test : string -> 'a -> 'b -> ('a -> 'b -> bool) -> unit