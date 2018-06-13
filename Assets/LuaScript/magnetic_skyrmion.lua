
-- Initial the magnetic by random
function GetMagneticByLatticeIndex(x, y)
    -- skyrmion at position 255, 255
    local rho = (x - 255) * (x - 255) + (y - 255) * (y- 255)
    -- radius 20
    local theta = math.pi * math.exp(- rho / (100 * 100))
    
    
    local phi = 0
    if x > 255 and y > 255 then
		phi = math.atan((y - 255)/(x - 255))
	end
	if x < 255 and y > 255 then
		phi = math.pi - math.atan(-(y - 255)/(x - 255))
	end		
	if x > 255 and y < 255 then
		phi = 2 * math.pi - math.atan(-(y - 255)/(x - 255))
	end	
	if x < 255 and y < 255 then
		phi = math.pi + math.atan((y - 255)/(x - 255))
	end		    

	local gamma = math.pi * 0.5
	local m = 1
	local g = 1
		
    local nx = math.cos(gamma + m * phi) * math.sin(theta)
    local ny = math.sin(gamma + m * phi) * math.sin(theta)
    local nz = g * math.cos(theta)

    return nx, ny, nz
end
