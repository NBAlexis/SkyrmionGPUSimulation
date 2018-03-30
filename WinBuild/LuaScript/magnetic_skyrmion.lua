
-- Initial the magnetic by random
function GetMagneticByLatticeIndex(x, y)
    -- skyrmion at position 100, 255
    local rho = (x - 100) * (x - 100) + (y - 255) * (y- 255)
    -- radius 20
    local theta = math.pi * math.exp(- rho / (20 * 20))
    local phi = math.atan2(x - 100, y - 255)

    local nx = math.cos(phi) * math.sin(theta)
    local ny = math.sin(phi) * math.sin(theta)
    local nz = math.cos(theta)

    return nx, ny, nz
end

-- Need to register the function
return { 
    GetMagneticByLatticeIndex = GetMagneticByLatticeIndex,
}
