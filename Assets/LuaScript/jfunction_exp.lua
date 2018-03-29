-- Exchange Strength is pin
function GetJValueByLatticeIndex(x, y)
    local j0 = 1
    local j1 = 1
    local j2 = 0.01
    -- Center point is 255
    -- math.sqrt((x - 255) * (x - 255) + (y - 255) * (y - 255))
    local rho = (x - 255) * (x - 255) + (y - 255) * (y - 255)

    return j0 + j1 * math.exp(-1.0 * j2 * rho)
end

-- Need to register the function
return { 
    GetJValueByLatticeIndex = GetJValueByLatticeIndex,
}
