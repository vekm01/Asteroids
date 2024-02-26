module Asteroids

open System

type Pos = float*float // Position vector
type Dir = float*float // Direction vector

let deltaT = 0.05
let size = 512

let mutable gameTime = 0.0 // Tracks time since game start. Increases by deltaT every time Update () is called.

let rec uniquePairs lst =
    match lst with
        [] -> []
        | x::rst -> (List.map (fun y -> (x, y)) rst) @ uniquePairs rst

let vecToAngle v =
    let x,y = v
    atan2 y x

let magnitude v =
    let x,y = v
    sqrt (x**2. + y**2.)

let rnd = System.Random ()

let randomFloat (min:float) (max:float) =
    let range = max - min
    min + (rnd.NextDouble ())*range

let angleToDirVec (angle) =
    let radians = angle*(2.*Math.PI/360.)
    (cos (radians), sin (radians))
    
let randomFloatPos min max =
        (randomFloat min max, randomFloat min max)

let rotate (v:Dir) (angle:float) = // Radians
    let (x,y) = v
    (x*cos(angle) - y*sin(angle), x*sin(angle) + y*cos(angle))

let distance (p1:Pos) (p2:Pos) =
    let x1, y1 = p1
    let x2, y2 = p2
    let deltaX, deltaY = x2-x1, y2-y1
    sqrt (deltaX**2.0 + deltaY**2.0)

let randomPosList (dist:float) (n:int) (min:float) (max:float) =
    let mutable lst = [randomFloatPos min max]
    while lst.Length < n do
        let p = randomFloatPos min max
        if not (List.exists (fun a -> distance a p < dist ) lst) then
            lst <- p::lst
    lst

let randomDirVec () =
    let angle = randomFloat 0.0 2*Math.PI
    (cos(angle), sin(angle))

type GameObject (speed: float, maxSpeed: float, position: Pos, dir : Dir, radius : float) =
    let mutable _speed = speed 
    let mutable _maxSpeed = maxSpeed
    let mutable _position = position
    let mutable _dir = dir
    let mutable _isHit = false
    let _timeCreated = gameTime

    member this.TimeCreated = _timeCreated

    member this.Hit 
        with get() = _isHit
        and set newHit = _isHit <- newHit

    member this.Speed 
        with get() = _speed
        and set newSpeed = _speed <- newSpeed
    
    member this.MaxSpeed
        with get() = _maxSpeed
        and set newMaxSpeed = _maxSpeed <- newMaxSpeed

    member this.Position 
        with get() = _position
        and set newPos = _position <- newPos
        
    member this.Dir 
        with get() = _dir
        and set newDir = _dir <- newDir

    member this.Radius = radius

    member this.NewPosition () = 
        let x1,y1 = _position
        let x2,y2 = _dir
        let mutable newX,newY = (x1 + deltaT*_speed*x2,y1 + deltaT*_speed*y2)
        // Wrap around when of out of bounds
        if newX < 0.0 then
            newX <- newX + float size
        if newX > float size then
            newX <- newX - float size
        if newY < 0.0 then
            newY <- newY + float size
        if newY > float size then
            newY <- newY - float size
        _position <- (newX, newY)
    
   
    override this.ToString () =
        sprintf "Pos: %A, TStamp: %A" this.Position this.TimeCreated

type Bullet (speed: float, maxSpeed: float, position: Pos, dir : Dir, radius : float) =
    inherit GameObject (speed, speed, position, dir, radius)
    
type Spaceship (acc: float, speed: float, maxSpeed: float, position: Pos, dir : Dir, radius : float) =
    inherit GameObject (speed, maxSpeed, position, dir, radius)
    let mutable _bullets = []

    member this.Bullets
        with get () = _bullets
        and set (newBuls) = _bullets <- newBuls 

    member this.Acceleration = acc
    
    member this.Accelerate () =
        let newSpeed = this.Speed + this.Acceleration * deltaT
        if newSpeed > maxSpeed then
            this.Speed <- maxSpeed
        else
            this.Speed <- newSpeed  
    
    member this.Shoot () = 
        let b = Bullet(60, 60, (fst this.Position, snd this.Position), this.Dir, 2.)
        _bullets <- b::_bullets
             
    member this.Turn (angle:float) = // Angle in radians
        this.Dir <- rotate this.Dir angle

type Asteroid (speed: float, maxSpeed: float, position: Pos, dir : Dir, radius : float) =
    inherit GameObject(speed, maxSpeed, position, dir, radius)

type Game (n:int) =
    let posList = randomPosList 64 (n+1) 0.0 size
    let mutable _spaceship = Spaceship (6.0, 0.0, 40.0, posList[0], randomDirVec (), 8.0)
    let mutable _asteroids =
        List.removeAt 0 posList // Removing spaceship's position from list of random positions and piping remainder onward
        |> List.map (fun p -> Asteroid (randomFloat 5.0 10.0, 10.0, p, randomDirVec (), 32.0))

    member this.Spaceship 
        with get () = _spaceship
        and set newSpaceship = _spaceship <- newSpaceship

    member this.Asteroids
        with get () = _asteroids
        and set newAsteroids = _asteroids <- newAsteroids
    
    member this.ObjDist (obj1:GameObject) (obj2:GameObject) =
        let x1,y1 = obj1.Position
        let x2,y2 = obj2.Position
        let deltaX, deltaY = x2-x1, y2-y1
        sqrt (deltaX**2.0 + deltaY**2.0)
    
    member this.Collisions () =

        // Find collision between spaceship and anything
        let shipAstCollided = List.exists (fun (a:Asteroid) -> distance _spaceship.Position a.Position < _spaceship.Radius + a.Radius) _asteroids
        let validBulletsForCollision = List.filter (fun (b:Bullet) -> gameTime - b.TimeCreated > 0.65) _spaceship.Bullets
        let shipBulCollided: bool = List.exists (fun (b:Bullet) -> this.ObjDist  _spaceship b < _spaceship.Radius + b.Radius) validBulletsForCollision
        if shipAstCollided || shipBulCollided then
            _spaceship.Hit <- true
            // This is game over 

        // Find collisions between asteroids and asteroids *(only ones who are not in grace period (5 s))
        let liveAsteroids = List.filter (fun (ast:Asteroid) -> gameTime - ast.TimeCreated > 5.0) _asteroids

        let astPairs = uniquePairs liveAsteroids
        let collidedAstPairs =
            List.filter (fun (a1:Asteroid, a2:Asteroid) -> this.ObjDist a1 a2 < a1.Radius + a2.Radius) astPairs
        let mutable collidedAsts =
            List.fold (fun acc (a1,a2) -> a1::(a2::acc)) [] collidedAstPairs
            |> List.distinct 

        // Find collisions between bullets and all asteroids
        let bulAstPairs = List.allPairs _spaceship.Bullets _asteroids
        let collidedBulAstPairs =
            List.filter (fun (bul:Bullet, ast:Asteroid) -> this.ObjDist bul ast < bul.Radius + ast.Radius) bulAstPairs
        let collidedBuls = List.map (fun (b,_) -> b) collidedBulAstPairs
        let collidedAsts2 = List.map (fun (_,a) -> a) collidedBulAstPairs
        collidedAsts <- List.distinct (collidedAsts2@collidedAsts)

        // Create new asteroids from collided asteroids (whose radius is greater than 8)
        let astsToDivide = List.filter (fun (ast:Asteroid) -> ast.Radius > 8.0) collidedAsts
        let child (parent:Asteroid) =
            Asteroid (randomFloat 5.0 10.0, 10.0, parent.Position, randomDirVec (), parent.Radius/2.0)
        let newAsts =
            List.fold (fun acc (ast:Asteroid) -> (child ast)::(child ast)::acc) [] astsToDivide

        // Update asteroid list
        let survivingAsts = List.filter (fun ast -> not (List.contains ast collidedAsts)) _asteroids
        _asteroids <- survivingAsts @ newAsts

        // Update bullet list
        let notCollidedBuls = List.filter (fun bul -> not (List.contains bul collidedBuls)) _spaceship.Bullets
        let survivingBuls = List.filter (fun (bul:Bullet) -> gameTime - bul.TimeCreated < 2.0 ) notCollidedBuls // Filter out bullets that have existed for more than 2 seconds
        _spaceship.Bullets <- survivingBuls
        
    member this.GameWon () =
        _asteroids.IsEmpty
    
    member this.Update () =
        if not _spaceship.Hit then // As long as spaceship isn't dead ...
            // Increment game time
            gameTime <- gameTime + deltaT

            // Update position of all objects
            _spaceship.NewPosition () // ... spaceship
            List.iter (fun (ast:Asteroid) -> ast.NewPosition ()) _asteroids // ... asteroids
            List.iter (fun (bul:Bullet) -> bul.NewPosition ()) _spaceship.Bullets // ... bullets

            //Check for and manage collisions
            this.Collisions ()

type Assert () =
    static member test (str:string) (a:'a) (b:'b) (f:'a->'b->bool) =
        printfn "%b: %s" (f a b) (str)