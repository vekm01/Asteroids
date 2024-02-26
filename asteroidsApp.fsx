#load "asteroids.fs"
#r "nuget:DIKU.Canvas, 2.0"
open Canvas
open Color
open Asteroids

type Pos = float*float
type State = Game

// Tests of game and objects

let s = Spaceship (10.0, 100.0, 110.0, (510.,510.), (1.,0.), 8.0)
let s1 = Spaceship (5., 0.0, 10.0, (5.,5.), (angleToDirVec (System.Math.PI/4.)), 8.0)
let g = Game (10)
let g1, g2 = Game (1), Game (1)
let g3 = Game (1)
let aI = Asteroid(10., 10.0, (1.,1.), (1.,0.), 32.0)
let sI = Spaceship(10.0, 0.0, 20.0, (10.,10.), (1.,0.), 8.0)
let gI = Game(10)

// Paragraph 1
Assert.test "Size of playable area is 512 by 512 pixels" size 512 (=)

let sInitPos = s.Position // (510, 510)
s.NewPosition ()
s.Turn (System.Math.PI/2.)
s.NewPosition () // Position is now (3, 3)
Assert.test "Playable area has the topology of a donut" s.Position sInitPos (<)

// Paragraph 2
Assert.test "The game starts with N large asteroids" 10 g.Asteroids.Length (=)
Assert.test "Asteroids have a max speed of 10 pixels per second" g.Asteroids[0].MaxSpeed 10.0 (=)

let g5 = Game (8)
let g6 = Game (8)
let g5AstPositions = List.map (fun (ast:Asteroid) -> ast.Position) g5.Asteroids
let g6AstPositions = List.map (fun (ast:Asteroid) -> ast.Position) g6.Asteroids
Assert.test "Asteroids start on random positions" g5AstPositions g6AstPositions (<>)

let notOverlapping =
    List.fold (fun acc (ast1:Asteroid, ast2:Asteroid) -> acc && (g5.ObjDist ast1 ast2 > 64.0)) true (uniquePairs g5.Asteroids)
Assert.test "Asteroids' positions are non-overlapping" notOverlapping true (=)

// Paragraph 3
Assert.test "The motion of objects is simulated in discrete timesteps (deltaT = 0.05 seconds)" deltaT 0.05 (=)

// Paragraph 4
let pos1 = aI.Position
aI.NewPosition()  
let pos2 = aI.Position
Assert.test "The asteroid's position is updated as expected" (fst pos2 - fst pos1, snd pos2 - snd pos1) ((0.5,0.)) (=)
let speed1 = aI.Speed
aI.NewPosition()
let speed2 = aI.Speed
Assert.test "Asteroid does not accelerate" speed1 speed2 (=)

// Paragraph 5
Assert.test "The spaceship starts with a speed of 0" g1.Spaceship.Speed 0.0 (=)
Assert.test "The spaceship starts at a random position" g1.Spaceship.Position g2.Spaceship.Position (<>)

let notOverlapping1 =
    List.fold (fun acc (a:Asteroid) -> acc && (g1.ObjDist g1.Spaceship a > 64.0)) true g1.Asteroids
Assert.test "The spaceship's position does not overlap with anything else" notOverlapping1 true (=)
Assert.test "The spaceships starts with a random orientation" g1.Spaceship.Dir g2.Spaceship.Dir (<>)
Assert.test "Orientation is a unit vector" (System.Math.Round (magnitude g1.Spaceship.Dir)) 1.0 (=)

// Paragraph 6
let currentDir = s.Dir
s.Turn (System.Math.PI/2.)
let newDir = s.Dir
Assert.test "The spaceship can turn" currentDir newDir (<>)

let currentSpeed = s.Speed
s.Accelerate ()
let newSpeed = s.Speed
Assert.test "The spaceship can accelerate" currentSpeed newSpeed (<)

// Paragraph 7
sI.Accelerate()
sI.Accelerate()
let spd = sI.Speed
Assert.test "The spaceship accelerates according to rule" spd 1. (=)

sI.NewPosition()
let pos = sI.Position
Assert.test "The spaceship's position is updated according to rule" pos ((10.05,10.)) (=)

// Paragraph 8
s.Shoot ()
Assert.test "The spaceship can shoot" s.Bullets.Length 1 (=)

g3.Spaceship <- Spaceship(0., 0.0, 20.0, (400.,400.), (-1.,0.), 8.0)
g3.Asteroids <- [Asteroid(0., 0.0, (10.,10.), (1.,0.), 8.0)]
g3.Spaceship.Shoot ()
for i = 0 to 40 do // 2 seconds later ...
    g3.Update ()
Assert.test "Bullets disappear after two seconds" g3.Spaceship.Bullets.Length 0 (=)

Assert.test "Bullet has same orientation as spaceship when shot" s.Dir s.Bullets.Head.Dir (=)

Assert.test "Bullet has same position as spaceship when shot" s.Position s.Bullets.Head.Position (=)

// Paragraph 9
let spaceshiptest = Spaceship(6.0, 5.0, 20.0, (0.,0.), randomDirVec (), 8.0)
let bullet = Bullet(20, 20, (100.,100.), randomDirVec (), 2.)
let ast =  Asteroid (randomFloat 5.0 10.0, 10.0, (100.,100.), randomDirVec (), 32.0)
Assert.test "The spaceship's radius is 8" (spaceshiptest.Radius) (8.) (=)
Assert.test "The bullet's radius is 2" (bullet.Radius) (2.) (=)
Assert.test "The asteroid's radius is 32" (ast.Radius) (32.) (=)

// Paragraph 10
gI.Spaceship <- Spaceship(10., 0.0, 20.0, (70.,10.), (-1.,0.), 8.0)
gI.Asteroids <- [Asteroid(10., 10.0, (1.,10.), (1.,0.), 32.0)]
Assert.test "The spaceship is not hit at start of game" (gI.Spaceship.Hit) (false) (=)
for i = 1 to 5 do
    gI.Spaceship.Accelerate()
for j = 1 to 50 do
    gI.Update()
Assert.test "The spaceship is hit and the game is over" (gI.Spaceship.Hit) (true) (=)
let objs = [gI.Spaceship :> GameObject]@(List.map (fun (a:Asteroid) -> a :> GameObject) gI.Asteroids)
let objsPositions = List.map (fun (obj:GameObject) -> obj.Position) objs
gI.Update ()
let newobjsPositions = List.map (fun (obj:GameObject) -> obj.Position) objs
Assert.test "Nothing moves when game is over" objsPositions newobjsPositions (=)

// Paragraph 11
let gameBul = Game (1)
gameBul.Spaceship <- Spaceship (0, 0, 0, (100.,100.), (1.,0.), 8)
gameBul.Asteroids <- [Asteroid (0, 0, (150,100), randomDirVec (), 32)]
for i = 0 to 6 do
    gameBul.Spaceship.Shoot ()
    gameBul.Update ()
for i = 0 to 250 do
    gameBul.Update ()
Assert.test "Bullets disappear when hitting an object" gameBul.Spaceship.Bullets.IsEmpty true (=)

// Paragraph 12
let game1 = Game(1)
game1.Spaceship <- Spaceship (0, 0, 0, (400.,400.), (1.,0.), 8)
let lst1 = [
    Asteroid (0., 0., (100.,100.),(1.,0.), 8.0);
    Asteroid (0., 0., (100.,100.), (-1.,0.), 8.);
]
game1.Asteroids <- lst1
for i = 0 to 100 do
    game1.Update()
Assert.test "Asteroids with radius 8 disappear after colliding" game1.Asteroids.IsEmpty true (=)

let game5 = Game (1)
game5.Asteroids <- [
    Asteroid (0, 10., (100,100),(1.,0.), 8.0)
    Asteroid (0, 10, (100,100), (-1.,0.), 32.0)]
    
game5.Spaceship <- Spaceship (0, 0, 0, (400.,400.), (1.,0.), 8)

for i = 0 to 100 do
    game5.Update ()

Assert.test "Astroids with radius greater than 8 split in two after colliding" game5.Asteroids.Length 2 (=)

Assert.test "The radius of the first child asteroid after collision is halved" game5.Asteroids[0].Radius 16. (=)
Assert.test "The radius of the second child asteroid after collision is halved" game5.Asteroids[1].Radius 16. (=)
Assert.test "The child asteroids have random velocities" game5.Asteroids[0].Speed game5.Asteroids[1].Speed (<>)

// Paragraph 13
let game4 = Game(10)
Assert.test "Game is not won when there are still asteroids left" (game4.GameWon ()) (false) (=)
let game3 = Game(0)
Assert.test "Game is won when there are no asteroids left" (game3.GameWon ()) (true) (=)

// Canvas
let game = Game (8)

/// <summary> Translates and rotates a primitive tree </summary>
/// <param name="pos"> Position for primitive tree to be translated to </param>
/// <param name="dir"> Direction of primitive tree to match with object it represents </param>
/// <param name="offset"> Optional angle offset </param>
/// <param name="tree"> Primitive tree representing desired object </param>
/// <returns> Primitive tree </returns>
let translateAndRotate (pos:Pos) (dir:Dir) (offset:float) (tree:PrimitiveTree) =
    translate (fst pos) (snd pos) tree
    |> Canvas.rotate 0.0 0.0 (offset + vecToAngle dir)

/// <summary> Creates visual representations of all game objects and prepares game for rendering </summary>
/// <param name="s"> State (Game type) </param>
/// <returns> A Picture type </returns>
let draw (s: State)  =
    // Spaceship
    let spaceship =
        filledPolygon white [(0.,-8.);(6.,5.29);(0.,4.);(-6.,5.29);(0.,-8.)]
        |> translateAndRotate s.Spaceship.Position s.Spaceship.Dir (System.Math.PI/2.)

    // Bullets
    let bullet = filledEllipse white 2.0 2.0
    let bulletPrims =
        List.map (fun (bul:Bullet) -> translate (fst bul.Position) (snd bul.Position) bullet) s.Spaceship.Bullets

    // Asteroids
    let bigAsteroid = piecewiseAffine white 1. [(-10.12, 30.36);(-30.36, 10.12);(-30.57, -9.47);(-10.,-10.);(-15.04,-28.24);(15.25,-28.13);(12.04,-13.06);(31.59,-5.09);(27.6,16.19);(13.94,28.8);(-10.12,30.36)]
    let smallAsteroid = piecewiseAffine white 1. [(-12.94, 9.41);(-14.03, -7.69);(-6.35, -6.81);(-5.94, -14.86);(6.94,-14.42);(6.31, -6.41);(14.3, -7.17);(12.44,10.);(0.,16.);(-12.94,9.41)]
    let miniAsteroid = piecewiseAffine white 1. [(-1.09,7.93);(-5.91,5.4);(-7.86,1.5);(-3.53,-1.51);(-4.84,-6.37);(1.18,-7.91);(4.0,-3.05);(7.82,-1.69);(4.85,6.36);(-1.09,7.93)]

    let lst8 = List.filter (fun (a: Asteroid) -> a.Radius = 8) s.Asteroids
    let lst16 = List.filter (fun (a: Asteroid) -> a.Radius = 16) s.Asteroids
    let lst32 = List.filter (fun (a: Asteroid) -> a.Radius = 32) s.Asteroids
    
    let miniAsts = List.map (fun (a:Asteroid) -> translateAndRotate a.Position a.Dir 0.0 miniAsteroid) lst8
    let smallAsts = List.map (fun (a:Asteroid) -> translateAndRotate a.Position a.Dir 0.0 smallAsteroid) lst16
    let bigAsts = List.map (fun (a:Asteroid) -> translateAndRotate a.Position a.Dir 0.0 bigAsteroid) lst32

    let mutable lstObjects = miniAsts @  smallAsts @ bigAsts @ [spaceship] @ bulletPrims

    if (game.GameWon()) then
        let won = Canvas.text green (makeFont "Microsoft Sans Serif" 36.0) "YOU WON" 
        let wonT = translate (float size/3.) (float size/2.) won
        lstObjects <- lstObjects @ [wonT]
    if game.Spaceship.Hit then
        let lost = Canvas.text red (makeFont "Microsoft Sans Serif" 36.0) "YOU LOST" 
        let lostT = translate (float size/3.) (float size/2.) lost
        lstObjects <- lstObjects @ [lostT]

    List.fold (fun t a -> onto t a) emptyTree lstObjects
            |> make

/// <summary> Makes game react to user input </summary>
/// <param name="s"> State (Game type) </param>
/// <param name="ev"> An event type </param>
/// <returns> A state option </returns>
let react (s: State) (ev: Event) : State option =
    match ev with
        UpArrow -> 
            s.Spaceship.Accelerate()
            Some(s)
        |LeftArrow ->
            s.Spaceship.Turn -(8.*System.Math.PI/360.)
            Some (s)
        |RightArrow ->
            s.Spaceship.Turn (8.*System.Math.PI/360.)
            Some (s)
        | Key ' ' ->
            s.Spaceship.Shoot ()
            Some (s)
        |TimerTick -> 
            s.Update()
            Some(s)
        | _ -> None 

let interval = Some (int (deltaT*1000.))
let initialState = (game)

// Makes the program interactive
interact "Game" size size interval draw react initialState