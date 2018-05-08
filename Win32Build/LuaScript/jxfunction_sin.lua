-- Applied electrical current
-- The direction of X-Axis is defined as the direction of electrical current so only jx is applied
-- Need two functions: 

-- t from 0 to 1
function GetJxValueInPeroid(t)
    return math.sin(2.0 * math.pi * t)
end

-- the length of the peroid (steps)
function GetJxPeroidLength()
    return 10000
end

-- Need to register the function
return { 
    GetJxValueInPeroid = GetJxValueInPeroid,
    GetJxPeroidLength = GetJxPeroidLength,
}
